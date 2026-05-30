class Patient {
  final String id;
  final String fullName;
  final int age;
  final String sex;
  final String condition;
  final String technologyLevel;

  Patient({
    required this.id,
    required this.fullName,
    required this.age,
    required this.sex,
    required this.condition,
    required this.technologyLevel,
  });

  factory Patient.fromJson(Map<String, dynamic> json) {
    return Patient(
      id: json['id'] ?? '',
      fullName: json['fullName'] ?? '',
      age: json['age'] ?? 0,
      sex: json['sex'] ?? '',
      condition: json['condition'] ?? '',
      technologyLevel: json['technologyLevel'] ?? '',
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'fullName': fullName,
      'age': age,
      'sex': sex,
      'condition': condition,
      'technologyLevel': technologyLevel,
    };
  }
}