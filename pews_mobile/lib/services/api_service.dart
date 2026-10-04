import 'dart:convert';
import 'package:http/http.dart' as http;
import '../constants.dart';
import '../models/api_models.dart';

class ApiService {
  final _client = http.Client();
  String get _base => Constants.apiBaseUrl;

  Map<String, String> _auth(String token) => {
        'Authorization': 'Bearer $token',
        'Content-Type': 'application/json',
      };

  Future<(bool, String, AuthResponse?)> login(String email, String password) async {
    try {
      final res = await _client.post(
        Uri.parse('$_base/api/auth/login'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({'email': email, 'password': password}),
      );
      if (res.statusCode == 200) {
        return (true, '', AuthResponse.fromJson(jsonDecode(res.body)));
      }
      final body = jsonDecode(res.body);
      return (false, (body['message'] as String?) ?? 'Invalid email or password', null as AuthResponse?);
    } catch (_) {
      return (false, 'Cannot connect to server. Is the backend running?', null as AuthResponse?);
    }
  }

  Future<(bool, String, AuthResponse?)> register({
    required String firstName,
    required String lastName,
    required String email,
    required String password,
    String? phone,
    String? city,
    String? region,
  }) async {
    try {
      final res = await _client.post(
        Uri.parse('$_base/api/auth/register'),
        headers: {'Content-Type': 'application/json'},
        body: jsonEncode({
          'firstName': firstName,
          'lastName': lastName,
          'email': email,
          'password': password,
          if (phone != null) 'phoneNumber': phone,
          if (city != null) 'city': city,
          if (region != null) 'region': region,
        }),
      );
      if (res.statusCode == 200) {
        return (true, '', AuthResponse.fromJson(jsonDecode(res.body)));
      }
      final body = jsonDecode(res.body);
      final errors = (body['errors'] as List?)?.join('\n') ?? (body['message'] as String?) ?? 'Registration failed';
      return (false, errors, null as AuthResponse?);
    } catch (_) {
      return (false, 'Cannot connect to server.', null as AuthResponse?);
    }
  }

  Future<DashboardStats?> getStats(String token) async {
    try {
      final res = await _client.get(
        Uri.parse('$_base/api/dashboard/stats'),
        headers: _auth(token),
      );
      if (res.statusCode == 200) return DashboardStats.fromJson(jsonDecode(res.body));
    } catch (_) {}
    return null;
  }

  Future<List<AlertItem>> getAlerts(String token, {int page = 1}) async {
    try {
      final res = await _client.get(
        Uri.parse('$_base/api/alerts?page=$page&pageSize=20'),
        headers: _auth(token),
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        return list.map((e) => AlertItem.fromJson(e)).toList();
      }
    } catch (_) {}
    return [];
  }

  Future<List<NotificationItem>> getNotifications(String token) async {
    try {
      final res = await _client.get(
        Uri.parse('$_base/api/notifications'),
        headers: _auth(token),
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        return list.map((e) => NotificationItem.fromJson(e)).toList();
      }
    } catch (_) {}
    return [];
  }

  Future<void> markNotificationRead(String token, int id) async {
    try {
      await _client.post(
        Uri.parse('$_base/api/notifications/$id/read'),
        headers: _auth(token),
      );
    } catch (_) {}
  }

  Future<void> markAllRead(String token) async {
    try {
      await _client.post(
        Uri.parse('$_base/api/notifications/read-all'),
        headers: _auth(token),
      );
    } catch (_) {}
  }

  Future<List<DisasterType>> getDisasterTypes(String token) async {
    try {
      final res = await _client.get(
        Uri.parse('$_base/api/reports/types'),
        headers: _auth(token),
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        return list.map((e) => DisasterType.fromJson(e)).toList();
      }
    } catch (_) {}
    return [];
  }

  Future<List<MyReportItem>> getMyReports(String token, {int page = 1}) async {
    try {
      final res = await _client.get(
        Uri.parse('$_base/api/reports?page=$page&pageSize=20'),
        headers: _auth(token),
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        return list.map((e) => MyReportItem.fromJson(e)).toList();
      }
    } catch (_) {}
    return [];
  }

  Future<List<Region>> getRegions(String token) async {
    try {
      final res = await _client.get(
        Uri.parse('$_base/api/reports/regions'),
        headers: _auth(token),
      );
      if (res.statusCode == 200) {
        final list = jsonDecode(res.body) as List;
        return list.map((e) => Region.fromJson(e)).toList();
      }
    } catch (_) {}
    return [];
  }

  Future<(bool, String)> submitReport({
    required String token,
    required String title,
    required String description,
    required int disasterTypeId,
    required int severity,
    String? location,
    int? regionId,
    List<PickedImage> images = const [],
  }) async {
    try {
      final uri = Uri.parse('$_base/api/reports');
      final request = http.MultipartRequest('POST', uri)
        ..headers['Authorization'] = 'Bearer $token'
        ..fields['Title']          = title
        ..fields['Description']    = description
        ..fields['DisasterTypeId'] = disasterTypeId.toString()
        ..fields['Severity']       = severity.toString();

      if (location != null) request.fields['Location'] = location;
      if (regionId != null) request.fields['RegionId'] = regionId.toString();

      for (final img in images) {
        request.files.add(http.MultipartFile.fromBytes('Images', img.bytes, filename: img.name));
      }

      final streamed = await _client.send(request);
      if (streamed.statusCode == 200 || streamed.statusCode == 201) {
        return (true, 'Report submitted successfully!');
      }
      return (false, 'Submission failed. Please try again.');
    } catch (_) {
      return (false, 'Network error. Check your connection.');
    }
  }

  Future<Profile?> getProfile(String token) async {
    try {
      final res = await _client.get(
        Uri.parse('$_base/api/profile'),
        headers: _auth(token),
      );
      if (res.statusCode == 200) return Profile.fromJson(jsonDecode(res.body));
    } catch (_) {}
    return null;
  }

  Future<(bool, String)> updateProfile({
    required String token,
    required String firstName,
    required String lastName,
    String? phoneNumber,
    String? address,
    String? city,
    String? region,
  }) async {
    try {
      final res = await _client.put(
        Uri.parse('$_base/api/profile'),
        headers: _auth(token),
        body: jsonEncode({
          'firstName': firstName,
          'lastName': lastName,
          if (phoneNumber != null) 'phoneNumber': phoneNumber,
          if (address != null) 'address': address,
          if (city != null) 'city': city,
          if (region != null) 'region': region,
        }),
      );
      if (res.statusCode == 200) return (true, 'Profile updated successfully.');
      final body = jsonDecode(res.body);
      final errors = (body['errors'] as List?)?.join('\n') ?? 'Update failed.';
      return (false, errors);
    } catch (_) {
      return (false, 'Network error. Check your connection.');
    }
  }

  Future<(bool, String)> changePassword({
    required String token,
    required String currentPassword,
    required String newPassword,
  }) async {
    try {
      final res = await _client.post(
        Uri.parse('$_base/api/profile/change-password'),
        headers: _auth(token),
        body: jsonEncode({
          'currentPassword': currentPassword,
          'newPassword': newPassword,
        }),
      );
      if (res.statusCode == 200) return (true, 'Password changed successfully.');
      final body = jsonDecode(res.body);
      final errors = (body['errors'] as List?)?.join('\n') ?? 'Password change failed.';
      return (false, errors);
    } catch (_) {
      return (false, 'Network error. Check your connection.');
    }
  }
}
