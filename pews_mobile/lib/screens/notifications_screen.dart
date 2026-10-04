import 'package:flutter/material.dart';
import '../constants.dart';
import '../models/api_models.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';

class NotificationsScreen extends StatefulWidget {
  const NotificationsScreen({super.key});
  @override
  State<NotificationsScreen> createState() => _NotificationsScreenState();
}

class _NotificationsScreenState extends State<NotificationsScreen> {
  final _api = ApiService();
  List<NotificationItem> _notifs  = [];
  bool _loading = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    setState(() => _loading = true);
    final token = await AuthStore.getToken();
    if (token == null) return;
    final items = await _api.getNotifications(token);
    if (mounted) setState(() { _notifs = items; _loading = false; });
  }

  Future<void> _markAllRead() async {
    final token = await AuthStore.getToken();
    if (token == null) return;
    await _api.markAllRead(token);
    _load();
  }

  Future<void> _markRead(NotificationItem n) async {
    if (n.isRead) return;
    final token = await AuthStore.getToken();
    if (token == null) return;
    await _api.markNotificationRead(token, n.id);
    _load();
  }

  @override
  Widget build(BuildContext context) {
    final unread = _notifs.where((n) => !n.isRead).length;
    return Scaffold(
      backgroundColor: Constants.bg,
      appBar: AppBar(
        backgroundColor: Constants.navy,
        foregroundColor: Colors.white,
        title: Text('Notifications${unread > 0 ? ' ($unread)' : ''}'),
        actions: [
          if (unread > 0)
            TextButton(
              onPressed: _markAllRead,
              child: const Text('Mark all read',
                style: TextStyle(color: Colors.white70, fontSize: 13)),
            ),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: Constants.navy))
          : RefreshIndicator(
              onRefresh: _load,
              child: _notifs.isEmpty
                  ? const Center(child: Text('No notifications yet.',
                      style: TextStyle(color: Colors.grey)))
                  : ListView.builder(
                      padding: const EdgeInsets.all(12),
                      itemCount: _notifs.length,
                      itemBuilder: (_, i) => _notifCard(_notifs[i]),
                    ),
            ),
    );
  }

  Widget _notifCard(NotificationItem n) {
    return Card(
      margin: const EdgeInsets.only(bottom: 8),
      elevation: n.isRead ? 1 : 3,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      color: n.isRead ? Colors.white : const Color(0xFFeff6ff),
      child: InkWell(
        onTap: () => _markRead(n),
        borderRadius: BorderRadius.circular(12),
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Row(crossAxisAlignment: CrossAxisAlignment.start, children: [
            Container(
              width: 40, height: 40,
              decoration: BoxDecoration(
                color: n.isRead
                    ? const Color(0xFFe2e8f0)
                    : Constants.navy.withValues(alpha: 0.1),
                borderRadius: BorderRadius.circular(20),
              ),
              child: Icon(Icons.notifications_outlined,
                color: n.isRead ? Colors.grey : Constants.navy, size: 20),
            ),
            const SizedBox(width: 12),
            Expanded(child: Column(
              crossAxisAlignment: CrossAxisAlignment.start,
              children: [
                Row(children: [
                  Expanded(child: Text(n.title,
                    style: TextStyle(
                      fontWeight: n.isRead ? FontWeight.normal : FontWeight.bold,
                      fontSize: 14, color: Constants.navy))),
                  if (!n.isRead)
                    Container(
                      width: 8, height: 8,
                      decoration: const BoxDecoration(
                        color: Constants.gold,
                        shape: BoxShape.circle,
                      ),
                    ),
                ]),
                const SizedBox(height: 4),
                Text(n.message,
                  style: const TextStyle(fontSize: 13, color: Color(0xFF4a5568))),
                const SizedBox(height: 6),
                Text(
                  n.createdAt.length > 10 ? n.createdAt.substring(0, 10) : n.createdAt,
                  style: const TextStyle(fontSize: 11, color: Colors.grey)),
              ],
            )),
          ]),
        ),
      ),
    );
  }
}
