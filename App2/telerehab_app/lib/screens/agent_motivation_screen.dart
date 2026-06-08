import 'dart:async';
import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter/services.dart';
import 'package:flutter_embed_unity/flutter_embed_unity.dart';

import '../models/patient.dart';
import '../models/therapy.dart';
import '../theme/app2_theme.dart';

class AgentMotivationScreen extends StatefulWidget {
  final Patient patient;
  final Therapy therapy;
  final bool completed;
  final int moodLevel;

  const AgentMotivationScreen({
    super.key,
    required this.patient,
    required this.therapy,
    required this.completed,
    required this.moodLevel,
  });

  @override
  State<AgentMotivationScreen> createState() => _AgentMotivationScreenState();
}

class _AgentMotivationScreenState extends State<AgentMotivationScreen>
    with SingleTickerProviderStateMixin {
  static const MethodChannel _audioPermissionChannel = MethodChannel(
    'com.pf3311.telerehab.agent/audio_permission',
  );

  late AnimationController _controller;
  late Animation<double> _scaleAnimation;
  final List<Timer> _unityMessageTimers = [];
  bool _microphonePermissionReady = false;

  bool get _supportsEmbeddedUnity =>
      !kIsWeb &&
      (defaultTargetPlatform == TargetPlatform.android ||
          defaultTargetPlatform == TargetPlatform.iOS);

  @override
  void initState() {
    super.initState();

    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 800),
    )..repeat(reverse: true);

    _scaleAnimation = Tween<double>(
      begin: 0.96,
      end: 1.04,
    ).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOut),
    );

    if (_supportsEmbeddedUnity) {
      _prepareMicrophonePermissionAndStartUnity();
    }
  }

  @override
  void dispose() {
    for (final timer in _unityMessageTimers) {
      timer.cancel();
    }
    _controller.dispose();
    super.dispose();
  }

  Future<void> _prepareMicrophonePermissionAndStartUnity() async {
    bool granted = true;

    if (defaultTargetPlatform == TargetPlatform.android) {
      granted = await _requestAndroidMicrophonePermission();
    }

    if (!mounted) return;

    setState(() => _microphonePermissionReady = granted);

    if (!granted) {
      return;
    }

    _unityMessageTimers
      ..add(Timer(const Duration(seconds: 1), _sendMessagesToUnity))
      ..add(Timer(const Duration(seconds: 2), _sendMessagesToUnity));
  }

  Future<bool> _requestAndroidMicrophonePermission() async {
    try {
      final granted = await _audioPermissionChannel.invokeMethod<bool>(
        'requestMicrophonePermission',
      );
      return granted ?? false;
    } catch (error) {
      debugPrint('No fue posible solicitar permiso de micrófono: $error');
      return false;
    }
  }

  void _sendMessagesToUnity() {
    if (!_microphonePermissionReady) return;

    final patientContext = jsonEncode({
      'patientId': widget.patient.id,
      'therapyId': widget.therapy.id,
      'patientName': widget.patient.fullName,
      'age': widget.patient.age,
      'sex': widget.patient.sex,
      'nationality': 'Costa Rica',
      'technologyLevel': widget.patient.technologyLevel,
      'condition': widget.patient.condition,
      'therapyName': widget.therapy.name,
      'mood': _moodDescription,
      'completedLastTherapy': widget.completed,
    });

    sendToUnity(
      'MotivationApiClient',
      'RequestMotivationMessage',
      patientContext,
    );
  }

  String get _moodDescription {
    if (widget.moodLevel <= 2) return 'cansado';
    if (widget.moodLevel >= 4) return 'motivado';
    return 'neutral';
  }

  Widget _buildAvatar(BuildContext context) {
    return ScaleTransition(
      scale: _scaleAnimation,
      child: CircleAvatar(
        radius: 78,
        backgroundColor: App2Palette.aqua,
        child: CircleAvatar(
          radius: 68,
          backgroundColor: App2Palette.violet,
          child: const Icon(
            Icons.smart_toy,
            size: 72,
            color: Colors.white,
          ),
        ),
      ),
    );
  }

  Widget _buildAgentVisual(BuildContext context) {
    if (!_supportsEmbeddedUnity) {
      return Center(child: _buildAvatar(context));
    }

    if (!_microphonePermissionReady) {
      return Container(
        height: 320,
        alignment: Alignment.center,
        decoration: BoxDecoration(
          color: Colors.black.withValues(alpha: 0.08),
          borderRadius: BorderRadius.circular(28),
        ),
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: FilledButton.icon(
            onPressed: _prepareMicrophonePermissionAndStartUnity,
            icon: const Icon(Icons.mic),
            label: const Text('Activar micrófono'),
          ),
        ),
      );
    }

    return ClipRRect(
      borderRadius: BorderRadius.circular(28),
      child: const SizedBox(
        height: 320,
        child: EmbedUnity(),
      ),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: const Text('Mensaje de mi agente'),
      ),
      body: Container(
        width: double.infinity,
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [
              App2Palette.softViolet,
              App2Palette.softAqua,
              App2Palette.canvas,
            ],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: ListView(
          padding: const EdgeInsets.all(20),
          children: [
            const SizedBox(height: 16),
            _buildAgentVisual(context),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: () => Navigator.pop(context),
              icon: const Icon(Icons.arrow_back),
              label: const Text('Volver a mi plan'),
            ),
          ],
        ),
      ),
    );
  }
}
