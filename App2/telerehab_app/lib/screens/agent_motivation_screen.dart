import 'dart:async';
import 'dart:convert';

import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';
import 'package:flutter_embed_unity/flutter_embed_unity.dart';

import '../models/patient.dart';
import '../models/therapy.dart';
import '../theme/app2_theme.dart';

class AgentMotivationScreen extends StatefulWidget {
  final Patient patient;
  final Therapy therapy;
  final bool completed;
  final int moodLevel;
  final String message;

  const AgentMotivationScreen({
    super.key,
    required this.patient,
    required this.therapy,
    required this.completed,
    required this.moodLevel,
    required this.message,
  });

  @override
  State<AgentMotivationScreen> createState() => _AgentMotivationScreenState();
}

class _AgentMotivationScreenState extends State<AgentMotivationScreen>
    with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _scaleAnimation;
  final List<Timer> _unityMessageTimers = [];

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
      _unityMessageTimers
        ..add(Timer(const Duration(seconds: 1), _sendMessagesToUnity))
        ..add(Timer(const Duration(seconds: 2), _sendMessagesToUnity));
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

  void _sendMessagesToUnity() {
    final patientContext = jsonEncode({
      'patientId': widget.patient.id,
      'patientName': widget.patient.fullName,
      'age': widget.patient.age,
      'sex': widget.patient.sex,
      'technologyLevel': widget.patient.technologyLevel,
      'condition': widget.patient.condition,
      'therapyName': widget.therapy.name,
      'mood': _moodDescription,
      'completedLastTherapy': widget.completed,
    });

    final avatarMessage = jsonEncode({
      'message': widget.message,
      'avatarProfile': _avatarProfile,
      'emotion': _emotion,
      'animation': _animation,
    });

    sendToUnity('AvatarBridge', 'ApplyPatientContext', patientContext);
    sendToUnity('AvatarBridge', 'ReceiveMessage', avatarMessage);
  }

  String get _moodDescription {
    if (widget.moodLevel <= 2) return 'cansado';
    if (widget.moodLevel >= 4) return 'motivado';
    return 'neutral';
  }

  String get _avatarProfile {
    if (widget.patient.age >= 60) return 'professional_health';
    if (widget.patient.technologyLevel.toLowerCase() == 'low') {
      return 'neutral_support';
    }
    if (widget.patient.sex.toUpperCase() == 'F') return 'friendly_female';
    if (widget.patient.sex.toUpperCase() == 'M') return 'friendly_male';
    return 'neutral_support';
  }

  String get _emotion {
    if (widget.completed) return 'happy';
    if (widget.moodLevel <= 2) return 'empathetic';
    return 'neutral';
  }

  String get _animation {
    if (widget.completed) return 'celebrate';
    if (widget.moodLevel <= 2) return 'empathetic';
    return 'talk';
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

    return ClipRRect(
      borderRadius: BorderRadius.circular(28),
      child: const SizedBox(
        height: 320,
        child: EmbedUnity(),
      ),
    );
  }

  Widget _buildSpeakingIndicator(BuildContext context) {
    return Row(
      mainAxisAlignment: MainAxisAlignment.center,
      children: [
        const Icon(
          Icons.auto_awesome,
          color: App2Palette.violet,
        ),
        const SizedBox(width: 8),
        Text(
          'Mensaje personalizado de tu agente',
          style: Theme.of(context).textTheme.bodyMedium,
        ),
      ],
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
            const SizedBox(height: 20),
            _buildSpeakingIndicator(context),
            const SizedBox(height: 24),
            Card(
              color: Colors.white,
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Column(
                  children: [
                    Text(
                      'Hola, ${widget.patient.fullName}',
                      style: Theme.of(context).textTheme.titleLarge?.copyWith(
                            color: App2Palette.deepNavy,
                            fontWeight: FontWeight.bold,
                          ),
                      textAlign: TextAlign.center,
                    ),
                    const SizedBox(height: 16),
                    Text(
                      widget.message,
                      style: Theme.of(context).textTheme.bodyLarge,
                      textAlign: TextAlign.center,
                    ),
                  ],
                ),
              ),
            ),
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
