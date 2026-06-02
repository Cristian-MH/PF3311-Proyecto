import 'package:flutter/material.dart';
import 'services/startup_screen.dart';
import 'theme/app2_theme.dart';

void main() {
  runApp(const TeleRehabTextApp());
}

class TeleRehabTextApp extends StatelessWidget {
  const TeleRehabTextApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'RehaBot',
      debugShowCheckedModeBanner: false,
      theme: buildApp2Theme(),
      home: const StartupScreen(),
    );
  }
}
