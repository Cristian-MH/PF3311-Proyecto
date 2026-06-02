import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import '../models/patient.dart';

class PatientStorageService {
  static const _patientKey = 'agent_app_registered_patient';
  static const Duration _storageTimeout = Duration(seconds: 5);

  Future<void> savePatient(Patient patient) async {
    final preferences = await SharedPreferences.getInstance().timeout(_storageTimeout);
    await preferences
        .setString(_patientKey, jsonEncode(patient.toJson()))
        .timeout(_storageTimeout);
  }

  Future<Patient?> loadPatient() async {
    final preferences = await SharedPreferences.getInstance().timeout(_storageTimeout);
    final patientJson = preferences.getString(_patientKey);

    if (patientJson == null) return null;

    try {
      return Patient.fromJson(jsonDecode(patientJson));
    } catch (_) {
      await clearPatient();
      return null;
    }
  }

  Future<void> clearPatient() async {
    final preferences = await SharedPreferences.getInstance().timeout(_storageTimeout);
    await preferences.remove(_patientKey).timeout(_storageTimeout);
  }
}
