import 'package:flutter/foundation.dart';
import 'package:flutter/material.dart';

class Constants {
  static String get apiBaseUrl {
    if (kIsWeb) return 'http://localhost:5201';
    return 'http://10.0.2.2:5201';
  }

  static const tokenKey    = 'pews_token';
  static const userIdKey   = 'pews_user_id';
  static const fullNameKey = 'pews_full_name';
  static const roleKey     = 'pews_role';
  static const expiryKey   = 'pews_expiry';

  static const navy = Color(0xFF1a3a5c);
  static const gold = Color(0xFFe8a020);
  static const bg   = Color(0xFFf0f4f8);
}
