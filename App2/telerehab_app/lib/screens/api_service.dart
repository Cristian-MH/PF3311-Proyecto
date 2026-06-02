import 'dart:convert';
import 'package:http/http.dart' as http;

import '../models/patient.dart';
import '../models/therapy.dart';
import '../models/therapy_log.dart';

class ApiService {
  static const String baseUrl =
      'https://pf3311-azf3h8a2a3gqcbeh.eastus2-01.azurewebsites.net/api';
  static const Duration _requestTimeout = Duration(seconds: 30);

  Future<Patient> createPatient(Patient patient) async {
    final response = await http.post(
      Uri.parse('$baseUrl/Patients'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(patient.toRegistrationJson()),
    ).timeout(_requestTimeout);

    if (response.statusCode != 201) {
      throw Exception('Error creating patient: ${response.body}');
    }

    return Patient.fromJson(jsonDecode(response.body));
  }

  Future<Patient?> getPatient(String patientId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/Patients/$patientId'),
    ).timeout(_requestTimeout);

    if (response.statusCode == 404) return null;

    if (response.statusCode != 200) {
      throw Exception('Error loading patient: ${response.body}');
    }

    return Patient.fromJson(jsonDecode(response.body));
  }

  Future<List<Therapy>> getTherapiesByPatient(String patientId) async {
    final response = await http.get(
      Uri.parse('$baseUrl/Therapies/patient/$patientId'),
    ).timeout(_requestTimeout);

    if (response.statusCode != 200) {
      throw Exception('Error loading therapies: ${response.body}');
    }

    final List<dynamic> data = jsonDecode(response.body);
    return data.map((item) => Therapy.fromJson(item)).toList();
  }

  Future<List<Therapy>> generateTherapies(String patientId) async {
    final response = await http.post(
      Uri.parse('$baseUrl/Therapies/generate/$patientId'),
    ).timeout(_requestTimeout);

    if (response.statusCode != 201) {
      throw Exception('Error generating therapies: ${response.body}');
    }

    final List<dynamic> data = jsonDecode(response.body);
    return data.map((item) => Therapy.fromJson(item)).toList();
  }

  Future<void> createTherapyLog(TherapyLog log) async {
    final response = await http.post(
      Uri.parse('$baseUrl/TherapyLogs'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode(log.toJson()),
    ).timeout(_requestTimeout);

    if (response.statusCode != 201) {
      throw Exception('Error creating therapy log: ${response.body}');
    }
  }

  Future<String> getMotivationMessage({
    required String patientId,
    required String therapyId,
  }) async {
    final response = await http.post(
      Uri.parse('$baseUrl/Motivation/message'),
      headers: {'Content-Type': 'application/json'},
      body: jsonEncode({
        'patientId': patientId,
        'therapyId': therapyId,
      }),
    ).timeout(_requestTimeout);

    if (response.statusCode != 200) {
      throw Exception('Error generating motivation message: ${response.body}');
    }

    final data = jsonDecode(response.body);
    return data['message'] ?? '';
  }
}
