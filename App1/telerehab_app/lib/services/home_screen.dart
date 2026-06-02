import 'package:flutter/material.dart';

import '../models/patient.dart';
import 'therapies_screen.dart';

class HomeScreen extends StatelessWidget {
  final Patient patient;

  const HomeScreen({
    super.key,
    required this.patient,
  });

  @override
  Widget build(BuildContext context) {
    final colorScheme = Theme.of(context).colorScheme;

    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: const [
            Icon(Icons.health_and_safety),
            SizedBox(width: 8),
            Text('RehaClassic'),
          ],
        ),
      ),
      body: Container(
        decoration: BoxDecoration(
          gradient: LinearGradient(
            colors: [
              Color.fromRGBO(
                colorScheme.primaryContainer.r.round(),
                colorScheme.primaryContainer.g.round(),
                colorScheme.primaryContainer.b.round(),
                0.25,
              ),
              colorScheme.surface,
            ],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Card(
              child: Padding(
                padding: const EdgeInsets.all(20),
                child: Row(
                  children: [
                    CircleAvatar(
                      radius: 30,
                      backgroundColor: Theme.of(context).colorScheme.primary,
                      child: const Icon(
                        Icons.person,
                        size: 32,
                        color: Colors.white,
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            '¡Bienvenido, ${patient.fullName}!',
                            style: Theme.of(context).textTheme.headlineSmall,
                          ),
                          const SizedBox(height: 8),
                          Text(
                            'Se generó una terapia inicial según tu información registrada. Revisa tus terapias y avanza paso a paso.',
                            style: Theme.of(context).textTheme.bodyLarge,
                          ),
                        ],
                      ),
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 16),
            Card(
              child: Padding(
                padding: const EdgeInsets.symmetric(
                  vertical: 18,
                  horizontal: 20,
                ),
                child: Column(
                  crossAxisAlignment: CrossAxisAlignment.start,
                  children: [
                    Text(
                      'Resumen del paciente',
                      style: Theme.of(context).textTheme.titleMedium,
                    ),
                    const SizedBox(height: 14),
                    Wrap(
                      spacing: 10,
                      runSpacing: 10,
                      children: [
                        _InfoChip(
                          label: 'Condición',
                          value: patient.condition,
                          icon: Icons.health_and_safety,
                        ),
                        _InfoChip(
                          label: 'Tecnología',
                          value: patient.technologyLevel,
                          icon: Icons.computer,
                        ),
                        _InfoChip(
                          label: 'Sexo',
                          value: patient.sex == 'M'
                              ? 'Masculino'
                              : patient.sex == 'F'
                                  ? 'Femenino'
                                  : 'Otro',
                          icon: Icons.wc,
                        ),
                      ],
                    ),
                  ],
                ),
              ),
            ),
            const SizedBox(height: 24),
            ElevatedButton.icon(
              onPressed: () {
                Navigator.push(
                  context,
                  MaterialPageRoute(
                    builder: (_) => TherapiesScreen(patient: patient),
                  ),
                );
              },
              icon: const Icon(Icons.medical_services),
              label: const Text('Ver terapias asignadas'),
            ),
          ],
        ),
      ),
    );
  }
}

class _InfoChip extends StatelessWidget {
  final String label;
  final String value;
  final IconData icon;

  const _InfoChip({
    required this.label,
    required this.value,
    required this.icon,
  });

  @override
  Widget build(BuildContext context) {
    return Chip(
      avatar: Icon(
        icon,
        size: 18,
        color: Theme.of(context).colorScheme.primary,
      ),
      label: Text('$label: $value'),
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      backgroundColor:
          Theme.of(context).colorScheme.primaryContainer.withAlpha(40),
    );
  }
}
