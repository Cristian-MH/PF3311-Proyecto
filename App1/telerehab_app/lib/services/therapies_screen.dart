import 'package:flutter/material.dart';

import '../models/patient.dart';
import '../models/therapy.dart';
import '../screens/api_service.dart';
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
            Icon(Icons.medical_services),
            SizedBox(width: 8),
            Text('Terapias asignadas'),
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
                children: const [
                  Icon(Icons.info_outline, size: 48),
                  SizedBox(height: 16),
                  Text(
                    'No hay terapias asignadas para este paciente.',
                    textAlign: TextAlign.center,
                  ),
                  SizedBox(height: 8),
                  Text(
                    'La terapia inicial se genera automáticamente después del registro. Intente refrescar la pantalla.',
                    textAlign: TextAlign.center,
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
                        const SizedBox(height: 16),
                        SizedBox(
                          width: double.infinity,
                          child: ElevatedButton.icon(
                            onPressed: () => _goToLogScreen(therapy),
                            icon: const Icon(Icons.check),
                            label: const Text('Registrar terapia'),
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
