import 'package:flutter/material.dart';
import 'services/startup_screen.dart';

void main() {
  runApp(const TeleRehabTextApp());
}

class TeleRehabTextApp extends StatelessWidget {
  const TeleRehabTextApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'TeleRehab',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        useMaterial3: true,
        colorSchemeSeed: Colors.indigo,
        scaffoldBackgroundColor: Colors.grey[50],
        appBarTheme: const AppBarTheme(
          centerTitle: true,
          elevation: 0,
        ),
        cardTheme: CardThemeData(
          shape: RoundedRectangleBorder(
            borderRadius: BorderRadius.circular(16),
          ),
          elevation: 2,
        ),
        elevatedButtonTheme: ElevatedButtonThemeData(
          style: ElevatedButton.styleFrom(
            shape: RoundedRectangleBorder(
              borderRadius: BorderRadius.circular(12),
            ),
            minimumSize: const Size.fromHeight(52),
          ),
        ),
      ),
      home: const StartupScreen(),
    );
  }
}
