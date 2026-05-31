import 'package:flutter_test/flutter_test.dart';
import 'package:shared_preferences/shared_preferences.dart';

import 'package:telerehab_app/main.dart';

void main() {
  testWidgets('App shows patient registration form', (WidgetTester tester) async {
    SharedPreferences.setMockInitialValues({});

    await tester.pumpWidget(const TeleRehabTextApp());
    await tester.pumpAndSettle();

    expect(find.text('Datos del paciente'), findsOneWidget);
  });
}
