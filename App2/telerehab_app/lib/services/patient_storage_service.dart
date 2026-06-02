import 'dart:convert';

import 'package:shared_preferences/shared_preferences.dart';

import '../models/patient.dart';

class PatientStorageService {
  static const _patientKey = 'agent_app_registered_patient';

  Future<void> savePatient(Patient patient) async {
    final preferences = await SharedPreferences.getInstance();
    await preferences.setString(_patientKey, jsonEncode(patient.toJson()));
  }

  Future<Patient?> loadPatient() async {
    final preferences = await SharedPreferences.getInstance();
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
    final preferences = await SharedPreferences.getInstance();
    await preferences.remove(_patientKey);
  }
}
