import 'dart:convert';
import 'package:http/http.dart' as http;

import '../models/patient.dart';
import '../models/therapy.dart';
import '../models/therapy_log.dart';

class ApiService {
  static const String baseUrl =
      'https://pf3311-azf3h8a2a3gqcbeh.eastus2-01.azurewebsites.net/api';

  Future<Patient> createPatient(Patient patient) async {
    final response = await http.post(
      Uri.parse('$baseUrl/Patients'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(patient.toJson()),
    );

    if (response.statusCode != 201) {
      throw Exception('Error creating patient: ${response.body}');
    }

    return Patient.fromJson(jsonDecode(response.body));
  }

  Future<List<Therapy>> getTherapiesByPatient(String patientId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/Therapies/patient/$patientId'),
    );

    if (response.statusCode != 200) {
      throw Exception('Error loading therapies: ${response.body}');
    }

    final List<dynamic> data = jsonDecode(response.body);
    return data.map((item) => Therapy.fromJson(item)).toList();
  }

  Future<void> createTherapyLog(TherapyLog log) async {
    final response = await http.post(
      Uri.parse('$baseUrl/TherapyLogs'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(log.toJson()),
    );

    if (response.statusCode != 201) {
      throw Exception('Error creating therapy log: ${response.body}');
    }
  }

  Future<String> getMotivationMessage({
    required String patientId,
    required String patientName,
    required int age,
    required String therapyName,
    required String mood,
    required bool completedLastTherapy,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/Motivation/message'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'patientId': patientId,
        'patientName': patientName,
        'age': age,
        'therapyName': therapyName,
        'mood': mood,
        'completedLastTherapy': completedLastTherapy,
      }),
    );

    if (response.statusCode != 200) {
      throw Exception('Error generating motivation message: ${response.body}');
    }

    final data = jsonDecode(response.body);
    return data['message'] ?? '';
  }
}