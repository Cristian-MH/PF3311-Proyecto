class Therapy {
  final String id;
  final String patientId;
  final String name;
  final String instructions;
  final int repetitions;
  final String frequency;

  Therapy({
    required this.id,
    required this.patientId,
    required this.name,
    required this.instructions,
    required this.repetitions,
    required this.frequency,
  });

  factory Therapy.fromJson(Map<String, dynamic> json) {
    return Therapy(
      id: json['id'] ?? '',
      patientId: json['patientId'] ?? '',
      name: json['name'] ?? '',
      instructions: json['instructions'] ?? '',
      repetitions: json['repetitions'] ?? 0,
      frequency: json['frequency'] ?? '',
    );
  }
}