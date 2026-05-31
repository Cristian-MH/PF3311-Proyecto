import 'package:flutter/material.dart';

import '../models/patient.dart';
import '../models/therapy.dart';
import '../models/therapy_log.dart';
import '../screens/api_service.dart';

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
  String? _feedbackMessage;
  String? _motivationMessage;

  Future<void> _saveLog() async {
    setState(() {
      _isSaving = true;
      _feedbackMessage = null;
      _motivationMessage = null;
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

      setState(() {
        _feedbackMessage = 'Tu avance fue registrado correctamente.';
      });

      try {
        final message = await _apiService.getMotivationMessage(
          patientId: widget.patient.id,
          therapyId: widget.therapy.id,
        );

        if (!mounted) return;

        setState(() {
          _motivationMessage = message;
        });
      } catch (_) {
        if (!mounted) return;

        setState(() {
          _motivationMessage =
              'Tu avance cuenta. Continúa a tu ritmo y detente si el dolor aumenta.';
        });
      }
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
        title: const Text('Registrar terapia'),
      ),
      body: ListView(
        padding: const EdgeInsets.all(16),
        children: [
          Card(
            child: Padding(
              padding: const EdgeInsets.all(16),
              child: Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text(
                    therapy.name,
                    style: Theme.of(context).textTheme.titleLarge,
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
            label: const Text('Guardar registro'),
          ),
          if (_feedbackMessage != null) ...[
            const SizedBox(height: 24),
            Card(
              color: Theme.of(context).colorScheme.secondaryContainer,
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Text(
                  _feedbackMessage!,
                  style: Theme.of(context).textTheme.bodyLarge,
                ),
              ),
            ),
          ],
          if (_motivationMessage != null) ...[
            const SizedBox(height: 24),
            Card(
              color: Theme.of(context).colorScheme.primaryContainer,
              child: Padding(
                padding: const EdgeInsets.all(16),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Mensaje motivacional',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 8),
                    Text(
                      _motivationMessage!,
                      style: Theme.of(context).textTheme.bodyLarge,
                    ),
                  ],
                ),
              ),
            ),
          ],
        ],
      ),
    );
  }
}
