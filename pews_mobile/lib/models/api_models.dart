import 'dart:typed_data';
import 'package:flutter/material.dart';

/// An image picked by the user, held in memory as bytes so it works on
/// every platform (dart:io.File has no filesystem backing on Flutter Web).
class PickedImage {
  final Uint8List bytes;
  final String name;
  PickedImage(this.bytes, this.name);
}

class AuthResponse {
  final String token;
  final String userId;
  final String email;
  final String fullName;
  final String role;
  final String expiresAt;

  AuthResponse.fromJson(Map<String, dynamic> j)
      : token     = j['token'],
        userId    = j['userId'],
        email     = j['email'],
        fullName  = j['fullName'],
        role      = j['role'],
        expiresAt = j['expiresAt'];
}

class DashboardStats {
  final int totalReports;
  final int activeAlerts;
  final int pendingReports;
  final int verifiedReports;
  final int unreadNotifications;

  DashboardStats.fromJson(Map<String, dynamic> j)
      : totalReports        = j['totalReports']        ?? 0,
        activeAlerts        = j['activeAlerts']        ?? 0,
        pendingReports      = j['pendingReports']      ?? 0,
        verifiedReports     = j['verifiedReports']     ?? 0,
        unreadNotifications = j['unreadNotifications'] ?? 0;
}

class AlertItem {
  final int    id;
  final String title;
  final String message;
  final String severity;
  final String category;
  final String region;
  final String publishedAt;

  AlertItem.fromJson(Map<String, dynamic> j)
      : id          = j['id'],
        title       = j['title']       ?? '',
        message     = j['message']     ?? '',
        severity    = j['severity']    ?? 'Low',
        category    = j['category']    ?? '',
        region      = j['region']      ?? '',
        publishedAt = j['publishedAt'] ?? '';

  Color get severityColor {
    switch (severity) {
      case 'Critical': return const Color(0xFF991b1b);
      case 'High':     return const Color(0xFFc2410c);
      case 'Medium':   return const Color(0xFFb45309);
      default:         return const Color(0xFF166534);
    }
  }
}

class NotificationItem {
  final int    id;
  final String title;
  final String message;
  final bool   isRead;
  final String createdAt;

  NotificationItem.fromJson(Map<String, dynamic> j)
      : id        = j['id'],
        title     = j['title']     ?? '',
        message   = j['message']   ?? '',
        isRead    = j['isRead']    ?? false,
        createdAt = j['createdAt'] ?? '';
}

class DisasterType {
  final int    id;
  final String name;
  DisasterType.fromJson(Map<String, dynamic> j)
      : id = j['id'], name = j['name'] ?? '';
}

class Region {
  final int    id;
  final String name;
  Region.fromJson(Map<String, dynamic> j)
      : id = j['id'], name = j['name'] ?? '';
}

class MyReportItem {
  final int    id;
  final String title;
  final String disasterType;
  final String severity;
  final String status;
  final String? region;
  final bool   isVerified;
  final int    imageCount;
  final String createdAt;

  MyReportItem.fromJson(Map<String, dynamic> j)
      : id           = j['id'],
        title        = j['title']        ?? '',
        disasterType = j['disasterType'] ?? '',
        severity     = j['severity']     ?? 'Low',
        status       = j['status']       ?? 'Pending',
        region       = j['region'],
        isVerified   = j['isVerified']   ?? false,
        imageCount   = j['imageCount']   ?? 0,
        createdAt    = j['createdAt']    ?? '';

  Color get severityColor {
    switch (severity) {
      case 'Critical': return const Color(0xFF991b1b);
      case 'High':     return const Color(0xFFc2410c);
      case 'Medium':   return const Color(0xFFb45309);
      default:         return const Color(0xFF166534);
    }
  }

  Color get statusColor {
    switch (status) {
      case 'Verified':   return const Color(0xFF065f46);
      case 'Resolved':   return const Color(0xFF166534);
      case 'Rejected':   return const Color(0xFF991b1b);
      case 'UnderReview': return const Color(0xFF1e40af);
      default:           return const Color(0xFF4a5568);
    }
  }

  Color get statusBg {
    switch (status) {
      case 'Verified':   return const Color(0xFFd1fae5);
      case 'Resolved':   return const Color(0xFFd4edda);
      case 'Rejected':   return const Color(0xFFfee2e2);
      case 'UnderReview': return const Color(0xFFdbeafe);
      default:           return const Color(0xFFe2e8f0);
    }
  }
}

class Profile {
  final String id;
  final String firstName;
  final String lastName;
  final String email;
  final String? phoneNumber;
  final String? address;
  final String? city;
  final String? region;
  final String createdAt;

  Profile.fromJson(Map<String, dynamic> j)
      : id          = j['id']        ?? '',
        firstName   = j['firstName'] ?? '',
        lastName    = j['lastName']  ?? '',
        email       = j['email']     ?? '',
        phoneNumber = j['phoneNumber'],
        address     = j['address'],
        city        = j['city'],
        region      = j['region'],
        createdAt   = j['createdAt'] ?? '';

  String get fullName => '$firstName $lastName';
}
