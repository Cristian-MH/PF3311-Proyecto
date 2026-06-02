import 'package:flutter/material.dart';

import '../models/patient.dart';
import '../models/therapy.dart';
import '../models/therapy_log.dart';
import '../screens/agent_motivation_screen.dart';
import '../screens/api_service.dart';
import '../theme/app2_theme.dart';

class TherapyLogScreen extends StatefulWidget {
  final Patient patient;
  final Therapy therapy;

  const TherapyLogScreen({
    super.key,
    required this.patient,
    required this.therapy,
  });

  @override
  State<TherapyLogScreen> createState() => _TherapyLogScreenState();
}

class _TherapyLogScreenState extends State<TherapyLogScreen> {
  final _apiService = ApiService();
  final _commentController = TextEditingController();

  bool _completed = true;
  int _moodLevel = 3;
  int _painLevel = 3;
  bool _isSaving = false;

  Future<void> _saveLog() async {
    setState(() {
      _isSaving = true;
    });

    try {
      final log = TherapyLog(
        patientId: widget.patient.id,
        therapyId: widget.therapy.id,
        completed: _completed,
        moodLevel: _moodLevel,
        painLevel: _painLevel,
        comment: _commentController.text.trim(),
      );

      await _apiService.createTherapyLog(log);

      if (!mounted) return;

      late final String motivationMessage;
      try {
        motivationMessage = await _apiService.getMotivationMessage(
          patientId: widget.patient.id,
          therapyId: widget.therapy.id,
        );
      } catch (_) {
        motivationMessage =
            'Tu avance cuenta. Continúa a tu ritmo y detente si el dolor aumenta.';
      }

      if (!mounted) return;

      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (_) => AgentMotivationScreen(
            patient: widget.patient,
            therapy: widget.therapy,
            completed: _completed,
            moodLevel: _moodLevel,
            message: motivationMessage,
          ),
        ),
      );
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error guardando registro: $e')),
      );
    } finally {
      if (mounted) {
        setState(() => _isSaving = false);
      }
    }
  }

  @override
  void dispose() {
    _commentController.dispose();
    super.dispose();
  }

  Widget _buildLevelSelector({
    required String label,
    required int value,
    required ValueChanged<int> onChanged,
  }) {
    return Column(
      crossAxisAlignment: CrossAxisAlignment.start,
      children: [
        Text('$label: $value'),
        Slider(
          value: value.toDouble(),
          min: 1,
          max: 5,
          divisions: 4,
          label: value.toString(),
          onChanged: (newValue) => onChanged(newValue.toInt()),
        ),
      ],
    );
  }

  @override
  Widget build(BuildContext context) {
    final therapy = widget.therapy;

    return Scaffold(
      appBar: AppBar(
        title: const Text('Registrar mi sesión'),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            color: App2Palette.softAqua,
            child: Padding(
              padding: const EdgeInsets.all(18),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  const Row(
                    children: [
                      Icon(Icons.smart_toy, color: App2Palette.teal),
                      SizedBox(width: 8),
                      Text(
                        'Ejercicio guiado',
                        style: TextStyle(
                          color: App2Palette.teal,
                          fontWeight: FontWeight.bold,
                        ),
                      ),
                    ],
                  ),
                  const SizedBox(height: 12),
                  Text(
                    therapy.name,
                    style: Theme.of(context).textTheme.titleLarge?.copyWith(
                          color: App2Palette.deepNavy,
                          fontWeight: FontWeight.bold,
                        ),
                  ),
                  const SizedBox(height: 8),
                  Text(therapy.instructions),
                  const SizedBox(height: 8),
                  Text('Repeticiones: ${therapy.repetitions}'),
                  Text('Frecuencia: ${therapy.frequency}'),
                ],
              ),
            ),
          ),
          const SizedBox(height: 16),
          SwitchListTile(
            tileColor: Colors.white,
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(18),
            ),
            title: const Text('¿Completó la terapia?'),
            value: _completed,
            onChanged: (value) {
              setState(() => _completed = value);
            },
          ),
          const SizedBox(height: 8),
          _buildLevelSelector(
            label: 'Nivel de ánimo',
            value: _moodLevel,
            onChanged: (value) {
              setState(() => _moodLevel = value);
            },
          ),
          const SizedBox(height: 8),
          _buildLevelSelector(
            label: 'Nivel de dolor',
            value: _painLevel,
            onChanged: (value) {
              setState(() => _painLevel = value);
            },
          ),
          const SizedBox(height: 12),
          TextField(
            controller: _commentController,
            maxLines: 3,
            decoration: const InputDecoration(
              labelText: 'Comentario',
              border: OutlineInputBorder(),
            ),
          ),
          const SizedBox(height: 24),
          ElevatedButton.icon(
            onPressed: _isSaving ? null : _saveLog,
            icon: _isSaving
                ? const SizedBox(
                    width: 16,
                    height: 16,
                    child: CircularProgressIndicator(strokeWidth: 2),
                  )
                : const Icon(Icons.save),
            label: Text(
              _isSaving
                  ? 'Tu agente está preparando un mensaje...'
                  : 'Guardar y hablar con mi agente',
            ),
          ),
        ],
      ),
    );
  }
}
