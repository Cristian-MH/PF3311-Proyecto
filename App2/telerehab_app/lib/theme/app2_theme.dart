import 'package:flutter/material.dart';

class App2Palette {
  static const deepNavy = Color(0xFF123047);
  static const teal = Color(0xFF008C95);
  static const aqua = Color(0xFF5DE2D7);
  static const violet = Color(0xFF7655C9);
  static const canvas = Color(0xFFF2FAF9);
  static const softAqua = Color(0xFFDDF8F5);
  static const softViolet = Color(0xFFEDE7FF);
}

ThemeData buildApp2Theme() {
  final colorScheme = ColorScheme.fromSeed(
    seedColor: App2Palette.teal,
    primary: App2Palette.teal,
    secondary: App2Palette.violet,
    surface: Colors.white,
  );

  return ThemeData(
    useMaterial3: true,
    colorScheme: colorScheme,
    scaffoldBackgroundColor: App2Palette.canvas,
    appBarTheme: const AppBarTheme(
      centerTitle: true,
      elevation: 0,
      backgroundColor: App2Palette.deepNavy,
      foregroundColor: Colors.white,
    ),
    cardTheme: CardThemeData(
      color: Colors.white,
      elevation: 0,
      shape: RoundedRectangleBorder(
        borderRadius: BorderRadius.circular(24),
        side: const BorderSide(color: Color(0x1F008C95)),
      ),
    ),
    inputDecorationTheme: InputDecorationTheme(
      filled: true,
      fillColor: Colors.white,
      border: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: const BorderSide(color: Color(0x33008C95)),
      ),
      enabledBorder: OutlineInputBorder(
        borderRadius: BorderRadius.circular(16),
        borderSide: const BorderSide(color: Color(0x33008C95)),
      ),
    ),
    elevatedButtonTheme: ElevatedButtonThemeData(
      style: ElevatedButton.styleFrom(
        backgroundColor: App2Palette.teal,
        foregroundColor: Colors.white,
        shape: RoundedRectangleBorder(
          borderRadius: BorderRadius.circular(18),
        ),
        minimumSize: const Size.fromHeight(54),
      ),
    ),
  );
}

