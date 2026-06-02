class TherapyLog {
  final String patientId;
  final String therapyId;
  final bool completed;
  final int moodLevel;
  final int painLevel;
  final String comment;

  TherapyLog({
    required this.patientId,
    required this.therapyId,
    required this.completed,
    required this.moodLevel,
    required this.painLevel,
    required this.comment,
  });

  Map<String, dynamic> toJson() {
    return {
      'patientId': patientId,
      'therapyId': therapyId,
      'completed': completed,
      'moodLevel': moodLevel,
      'painLevel': painLevel,
      'comment': comment,
    };
  }
}