import 'package:flutter/material.dart';

import '../screens/api_service.dart';
import '../theme/app2_theme.dart';
import 'home_screen.dart';
import 'patient_form_screen.dart';
import 'patient_storage_service.dart';

class StartupScreen extends StatefulWidget {
  const StartupScreen({super.key});

  @override
  State<StartupScreen> createState() => _StartupScreenState();
}

class _StartupScreenState extends State<StartupScreen> {
  final _apiService = ApiService();
  final _patientStorageService = PatientStorageService();

  bool _hasConnectionError = false;
  bool _isLoading = false;

  @override
  void initState() {
    super.initState();
    _loadPatient();
  }

  Future<void> _loadPatient() async {
    if (_isLoading) return;

    try {
      setState(() {
        _hasConnectionError = false;
        _isLoading = true;
      });

      final storedPatient = await _patientStorageService.loadPatient();

      if (!mounted) return;

      if (storedPatient == null) {
        _open(const PatientFormScreen());
        return;
      }

      final patient = await _apiService.getPatient(storedPatient.id);

      if (!mounted) return;

      if (patient == null) {
        await _patientStorageService.clearPatient();

        if (!mounted) return;

        _open(const PatientFormScreen());
        return;
      }

      await _patientStorageService.savePatient(patient);

      if (!mounted) return;

      _open(HomeScreen(patient: patient));
    } catch (_) {
      if (!mounted) return;

      setState(() => _hasConnectionError = true);
    } finally {
      if (mounted) {
        setState(() => _isLoading = false);
      }
    }
  }

  void _open(Widget screen) {
    Navigator.pushReplacement(
      context,
      MaterialPageRoute(builder: (_) => screen),
    );
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: App2Palette.deepNavy,
      body: Center(
        child: Padding(
          padding: const EdgeInsets.all(24),
          child: _hasConnectionError
              ? Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    const Icon(Icons.cloud_off, size: 48, color: Colors.white),
                    const SizedBox(height: 16),
                    const Text(
                      'No fue posible preparar la aplicación. Verifica tu '
                      'conexión e inténtalo nuevamente.',
                      textAlign: TextAlign.center,
                      style: TextStyle(color: Colors.white),
                    ),
                    const SizedBox(height: 16),
                    ElevatedButton(
                      onPressed: _loadPatient,
                      child: const Text('Reintentar'),
                    ),
                  ],
                )
              : const Column(
                  mainAxisSize: MainAxisSize.min,
                  children: [
                    Icon(
                      Icons.smart_toy,
                      size: 72,
                      color: App2Palette.aqua,
                    ),
                    SizedBox(height: 20),
                    CircularProgressIndicator(color: App2Palette.aqua),
                    SizedBox(height: 20),
                    Text(
                      'Preparando tu experiencia guiada...',
                      style: TextStyle(color: Colors.white),
                    ),
                  ],
                ),
        ),
      ),
    );
  }
}
