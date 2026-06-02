import 'package:flutter/material.dart';

import '../models/patient.dart';
import '../theme/app2_theme.dart';
import 'therapies_screen.dart';

class HomeScreen extends StatelessWidget {
  final Patient patient;

  const HomeScreen({
    super.key,
    required this.patient,
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: const [
            Icon(Icons.support_agent),
            SizedBox(width: 8),
            Text('RehaBot'),
          ],
        ),
      ),
      body: Container(
        decoration: const BoxDecoration(
          gradient: LinearGradient(
            colors: [
              App2Palette.softAqua,
              App2Palette.canvas,
            ],
            begin: Alignment.topCenter,
            end: Alignment.bottomCenter,
          ),
        ),
        child: ListView(
          padding: const EdgeInsets.all(16),
          children: [
            Container(
              decoration: BoxDecoration(
                gradient: const LinearGradient(
                  colors: [
                    App2Palette.deepNavy,
                    App2Palette.teal,
                  ],
                ),
                borderRadius: BorderRadius.circular(28),
              ),
              child: Padding(
                padding: const EdgeInsets.all(24),
                child: Row(
                  children: [
                    Container(
                      padding: const EdgeInsets.all(14),
                      decoration: const BoxDecoration(
                        color: App2Palette.aqua,
                        shape: BoxShape.circle,
                      ),
                      child: const Icon(
                        Icons.smart_toy,
                        size: 36,
                        color: App2Palette.deepNavy,
                      ),
                    ),
                    const SizedBox(width: 16),
                    Expanded(
                      child: Column(
                        crossAxisAlignment: CrossAxisAlignment.start,
                        children: [
                          Text(
                            'Hola, ${patient.fullName}',
                            style: Theme.of(context).textTheme.headlineSmall?.copyWith(
                                  color: Colors.white,
                                  fontWeight: FontWeight.bold,
                                ),
                          ),
                          const SizedBox(height: 8),
                          const Text(
                            'Tu agente de rehabilitación preparó un plan personalizado y te acompañará después de cada sesión.',
                            style: TextStyle(
                              color: Colors.white,
                              height: 1.35,
                            ),
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
              color: App2Palette.softViolet,
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
              label: const Text('Explorar mi plan'),
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
        color: App2Palette.violet,
      ),
      label: Text('$label: $value'),
      padding: const EdgeInsets.symmetric(horizontal: 10, vertical: 6),
      backgroundColor: Colors.white.withAlpha(210),
    );
  }
}
