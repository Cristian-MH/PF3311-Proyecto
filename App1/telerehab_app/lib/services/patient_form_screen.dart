import 'package:flutter/material.dart';
import 'package:flutter/services.dart';

import '../models/patient.dart';
import '../screens/api_service.dart';
import 'home_screen.dart';
import 'patient_storage_service.dart';

class PatientFormScreen extends StatefulWidget {
  const PatientFormScreen({super.key});

  @override
  State<PatientFormScreen> createState() => _PatientFormScreenState();
}

class _PatientFormScreenState extends State<PatientFormScreen> {
  final _formKey = GlobalKey<FormState>();
  final _apiService = ApiService();
  final _patientStorageService = PatientStorageService();

  final _nameController = TextEditingController();
  final _ageController = TextEditingController();
  final _conditionController = TextEditingController();

  String _sex = 'M';
  String _technologyLevel = 'medium';
  bool _isLoading = false;

  Future<void> _submit() async {
    if (!_formKey.currentState!.validate()) return;

    setState(() => _isLoading = true);

    try {
      final patient = Patient(
        id: '',
        fullName: _nameController.text.trim(),
        age: int.parse(_ageController.text.trim()),
        sex: _sex,
        condition: _conditionController.text.trim(),
        technologyLevel: _technologyLevel,
      );

      final createdPatient = await _apiService.createPatient(patient);
      await _apiService.generateTherapies(createdPatient.id);
      await _patientStorageService.savePatient(createdPatient);

      if (!mounted) return;

      Navigator.pushReplacement(
        context,
        MaterialPageRoute(
          builder: (_) => HomeScreen(patient: createdPatient),
        ),
      );
    } catch (e) {
      if (!mounted) return;

      ScaffoldMessenger.of(context).showSnackBar(
        SnackBar(content: Text('Error: $e')),
      );
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  @override
  void dispose() {
    _nameController.dispose();
    _ageController.dispose();
    _conditionController.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        title: Row(
          mainAxisSize: MainAxisSize.min,
          children: const [
            Icon(Icons.health_and_safety),
            SizedBox(width: 10),
            Text('RehaClassic'),
          ],
        ),
      ),
      body: SafeArea(
        child: Padding(
          padding: const EdgeInsets.all(16),
          child: _isLoading
              ? const Center(
                  child: Column(
                    mainAxisSize: MainAxisSize.min,
                    children: [
                      CircularProgressIndicator(),
                      SizedBox(height: 20),
                      Text(
                        'Registrando paciente y generando su terapia personalizada...',
                        textAlign: TextAlign.center,
                      ),
                    ],
                  ),
                )
              : SingleChildScrollView(
                  child: Column(
                    crossAxisAlignment: CrossAxisAlignment.stretch,
                    children: [
                      Card(
                        child: Padding(
                          padding: const EdgeInsets.all(20),
                          child: Column(
                            crossAxisAlignment: CrossAxisAlignment.start,
                            children: [
                              Row(
                                crossAxisAlignment: CrossAxisAlignment.center,
                                children: [
                                  CircleAvatar(
                                    radius: 26,
                                    backgroundColor: Theme.of(context).colorScheme.primary,
                                    child: const Icon(
                                      Icons.health_and_safety,
                                      size: 28,
                                      color: Colors.white,
                                    ),
                                  ),
                                  const SizedBox(width: 16),
                                  Expanded(
                                    child: Column(
                                      crossAxisAlignment: CrossAxisAlignment.start,
                                      children: [
                                        Text(
                                          'RehaClassic',
                                          style: Theme.of(context).textTheme.titleLarge,
                                        ),
                                        const SizedBox(height: 6),
                                        Text(
                                          'Completa tu registro para personalizar el tratamiento',
                                          style: Theme.of(context).textTheme.titleMedium,
                                        ),
                                      ],
                                    ),
                                  ),
                                ],
                              ),
                              const SizedBox(height: 12),
                              Text(
                                'Los datos que ingreses nos ayudarán a enviar las terapias adecuadas.',
                                style: Theme.of(context).textTheme.bodyLarge,
                              ),
                            ],
                          ),
                        ),
                      ),
                      const SizedBox(height: 18),
                      Card(
                        child: Padding(
                          padding: const EdgeInsets.all(18),
                          child: Form(
                            key: _formKey,
                            autovalidateMode: AutovalidateMode.onUserInteraction,
                            child: Column(
                              crossAxisAlignment: CrossAxisAlignment.stretch,
                              children: [
                                Text(
                                  'Datos del paciente',
                                  style: Theme.of(context).textTheme.titleMedium,
                                ),
                                const SizedBox(height: 16),
                                TextFormField(
                                  controller: _nameController,
                                  textInputAction: TextInputAction.next,
                                  decoration: const InputDecoration(
                                    labelText: 'Nombre completo',
                                    border: OutlineInputBorder(),
                                    prefixIcon: Icon(Icons.person),
                                    hintText: 'Ej. María Pérez',
                                  ),
                                  validator: (value) {
                                    if (value == null || value.trim().isEmpty) {
                                      return 'El nombre es requerido';
                                    }
                                    if (value.trim().length < 3) {
                                      return 'Ingrese al menos 3 caracteres';
                                    }
                                    return null;
                                  },
                                ),
                                const SizedBox(height: 14),
                                TextFormField(
                                  controller: _ageController,
                                  keyboardType: TextInputType.number,
                                  textInputAction: TextInputAction.next,
                                  inputFormatters: [FilteringTextInputFormatter.digitsOnly],
                                  decoration: const InputDecoration(
                                    labelText: 'Edad',
                                    border: OutlineInputBorder(),
                                    prefixIcon: Icon(Icons.calendar_today),
                                    hintText: 'Edad en años',
                                  ),
                                  validator: (value) {
                                    final age = int.tryParse(value ?? '');
                                    if (age == null || age <= 0) {
                                      return 'Ingrese una edad válida';
                                    }
                                    if (age > 120) {
                                      return 'Ingrese una edad realista';
                                    }
                                    return null;
                                  },
                                ),
                                const SizedBox(height: 14),
                                DropdownButtonFormField<String>(
                                  initialValue: _sex,
                                  decoration: const InputDecoration(
                                    labelText: 'Sexo',
                                    border: OutlineInputBorder(),
                                    prefixIcon: Icon(Icons.wc),
                                  ),
                                  items: const [
                                    DropdownMenuItem(value: 'M', child: Text('Masculino')),
                                    DropdownMenuItem(value: 'F', child: Text('Femenino')),
                                    DropdownMenuItem(value: 'O', child: Text('Otro')),
                                  ],
                                  onChanged: (value) {
                                    if (value != null) setState(() => _sex = value);
                                  },
                                ),
                                const SizedBox(height: 14),
                                TextFormField(
                                  controller: _conditionController,
                                  textInputAction: TextInputAction.next,
                                  decoration: const InputDecoration(
                                    labelText: 'Condición o terapia principal',
                                    border: OutlineInputBorder(),
                                    prefixIcon: Icon(Icons.medical_services),
                                    hintText: 'Ej. Rehabilitación de rodilla',
                                  ),
                                  validator: (value) {
                                    if (value == null || value.trim().isEmpty) {
                                      return 'La condición es requerida';
                                    }
                                    return null;
                                  },
                                ),
                                const SizedBox(height: 14),
                                DropdownButtonFormField<String>(
                                  initialValue: _technologyLevel,
                                  decoration: const InputDecoration(
                                    labelText: 'Nivel de familiaridad tecnológica',
                                    border: OutlineInputBorder(),
                                    prefixIcon: Icon(Icons.language),
                                  ),
                                  items: const [
                                    DropdownMenuItem(value: 'low', child: Text('Bajo')),
                                    DropdownMenuItem(value: 'medium', child: Text('Medio')),
                                    DropdownMenuItem(value: 'high', child: Text('Alto')),
                                  ],
                                  onChanged: (value) {
                                    if (value != null) {
                                      setState(() => _technologyLevel = value);
                                    }
                                  },
                                ),
                                const SizedBox(height: 24),
                                ElevatedButton.icon(
                                  onPressed: _submit,
                                  icon: const Icon(Icons.check_circle_outline),
                                  label: const Text('Continuar'),
                                ),
                              ],
                            ),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
        ),
      ),
    );
  }
}
