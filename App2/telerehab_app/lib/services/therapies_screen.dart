import 'package:flutter/material.dart';

import '../models/patient.dart';
import '../models/therapy.dart';
import '../screens/api_service.dart';
import '../theme/app2_theme.dart';
import 'therapy_log_screen.dart';

class TherapiesScreen extends StatefulWidget {
  final Patient patient;

  const TherapiesScreen({
    super.key,
    required this.patient,
  });

  @override
  State<TherapiesScreen> createState() => _TherapiesScreenState();
}

class _TherapiesScreenState extends State<TherapiesScreen> {
  final _apiService = ApiService();

  late Future<List<Therapy>> _therapiesFuture;

  @override
  void initState() {
    super.initState();
    _therapiesFuture = _apiService.getTherapiesByPatient(widget.patient.id);
  }

  Future<void> _refresh() async {
    setState(() {
      _therapiesFuture = _apiService.getTherapiesByPatient(widget.patient.id);
    });

    await _therapiesFuture;
  }

  Future<void> _generateTherapies() async {
    try {
      await _apiService.generateTherapies(widget.patient.id);
      await _refresh();
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error generando terapias: $e')),
      );
    }
  }

  void _goToLogScreen(Therapy therapy) {
    Navigator.push(
      context,
      MaterialPageRoute(
        builder: (_) => TherapyLogScreen(
          patient: widget.patient,
          therapy: therapy,
        ),
      ),
    ).then((_) => _refresh());
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: const [
            Icon(Icons.route),
            SizedBox(width: 8),
            Text('Mi ruta de ejercicios'),
          ],
        ),
      ),
      body: RefreshIndicator(
        onRefresh: _refresh,
        child: FutureBuilder<List<Therapy>>(
          future: _therapiesFuture,
          builder: (context, snapshot) {
            if (snapshot.connectionState == ConnectionState.waiting) {
              return const Center(child: CircularProgressIndicator());
            }

            if (snapshot.hasError) {
              return ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  Text('Error: ${snapshot.error}'),
                  const SizedBox(height: 12),
                  ElevatedButton(
                    onPressed: _refresh,
                    child: const Text('Reintentar'),
                  ),
                ],
              );
            }

            final therapies = snapshot.data ?? [];

            if (therapies.isEmpty) {
              return ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  const Icon(Icons.info_outline, size: 48),
                  const SizedBox(height: 16),
                  const Text(
                    'No hay terapias asignadas para este paciente.',
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 8),
                  const Text(
                    'Puede intentar generar nuevamente el plan personalizado.',
                    textAlign: TextAlign.center,
                  ),
                  const SizedBox(height: 16),
                  ElevatedButton.icon(
                    onPressed: _generateTherapies,
                    icon: const Icon(Icons.auto_awesome),
                    label: const Text('Generar terapias'),
                  ),
                ],
              );
            }

            return ListView.separated(
              padding: const EdgeInsets.all(16),
              itemCount: therapies.length,
              separatorBuilder: (context, _) => const SizedBox(height: 12),
              itemBuilder: (context, index) {
                final therapy = therapies[index];

                return Card(
                  child: Padding(
                    padding: const EdgeInsets.all(18),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.start,
                      children: [
                        Row(
                          children: [
                            CircleAvatar(
                              backgroundColor: App2Palette.softAqua,
                              foregroundColor: App2Palette.teal,
                              child: Text('${index + 1}'),
                            ),
                            const SizedBox(width: 12),
                            Expanded(
                              child: Text(
                                therapy.name,
                                style: Theme.of(context).textTheme.titleLarge?.copyWith(
                                      color: App2Palette.deepNavy,
                                      fontWeight: FontWeight.bold,
                                    ),
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 14),
                        Text(therapy.instructions),
                        const SizedBox(height: 14),
                        Wrap(
                          spacing: 8,
                          runSpacing: 8,
                          children: [
                            Chip(
                              avatar: const Icon(Icons.repeat, size: 18),
                              label: Text('${therapy.repetitions} repeticiones'),
                              backgroundColor: App2Palette.softAqua,
                            ),
                            Chip(
                              avatar: const Icon(Icons.schedule, size: 18),
                              label: Text(therapy.frequency),
                              backgroundColor: App2Palette.softViolet,
                            ),
                          ],
                        ),
                        const SizedBox(height: 16),
                        SizedBox(
                          width: double.infinity,
                          child: ElevatedButton.icon(
                            onPressed: () => _goToLogScreen(therapy),
                            icon: const Icon(Icons.play_arrow),
                            label: const Text('Registrar mi sesión'),
                          ),
                        ),
                      ],
                    ),
                  ),
                );
              },
            );
          },
        ),
      ),
    );
  }
}
