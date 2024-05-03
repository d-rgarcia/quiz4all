import 'package:flutter/material.dart';

class MyTextField extends StatelessWidget {
  final controller;
  final String hintText;
  final bool obscureText;

  const MyTextField({
    super.key,
    required this.controller,
    required this.hintText,
    required this.obscureText,
  });

  @override
  Widget build(BuildContext context) {
    
    final enableNoFocusColor = Theme.of(context).colorScheme.inversePrimary;
    final enableFocusColor = Theme.of(context).colorScheme.primary;
    final fillColor = Theme.of(context).colorScheme.onPrimary;
    final hintColor = Theme.of(context).colorScheme.primary;

    return Padding(
      padding: const EdgeInsets.symmetric(horizontal: 25.0),
      child: TextField(
        controller: controller,
        obscureText: obscureText,
        decoration: InputDecoration(
            enabledBorder: OutlineInputBorder(
              borderSide: BorderSide(color: enableNoFocusColor),
            ),
            focusedBorder: OutlineInputBorder(
              borderSide: BorderSide(color:  enableFocusColor),
            ),
            fillColor: fillColor,
            filled: true,
            hintText: hintText,
            hintStyle: TextStyle(color: hintColor)),
      ),
    );
  }
}