import 'package:flutter_secure_storage/flutter_secure_storage.dart';
import '../constants.dart';
import '../models/api_models.dart';

class AuthStore {
  static const _storage = FlutterSecureStorage();

  static Future<void> save(AuthResponse auth) async {
    await _storage.write(key: Constants.tokenKey,    value: auth.token);
    await _storage.write(key: Constants.userIdKey,   value: auth.userId);
    await _storage.write(key: Constants.fullNameKey, value: auth.fullName);
    await _storage.write(key: Constants.roleKey,     value: auth.role);
    await _storage.write(key: Constants.expiryKey,   value: auth.expiresAt);
  }

  static Future<String?> getToken() => _storage.read(key: Constants.tokenKey);
  static Future<String?> getFullName() => _storage.read(key: Constants.fullNameKey);
  static Future<String?> getRole() => _storage.read(key: Constants.roleKey);

  static Future<bool> isLoggedIn() async {
    final token  = await _storage.read(key: Constants.tokenKey);
    final expiry = await _storage.read(key: Constants.expiryKey);
    if (token == null || expiry == null) return false;
    final exp = DateTime.tryParse(expiry);
    return exp != null && exp.isAfter(DateTime.now());
  }

  static Future<void> clear() => _storage.deleteAll();
}
