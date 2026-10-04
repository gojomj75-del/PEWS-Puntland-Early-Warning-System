import 'package:flutter/material.dart';
import '../constants.dart';
import '../models/api_models.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';

class AlertsScreen extends StatefulWidget {
  const AlertsScreen({super.key});
  @override
  State<AlertsScreen> createState() => _AlertsScreenState();
}

class _AlertsScreenState extends State<AlertsScreen> {
  final _api   = ApiService();
  List<AlertItem> _alerts  = [];
  bool _loading = true;
  int  _page    = 1;
  bool _hasMore = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load({bool reset = false}) async {
    if (reset) { _page = 1; _hasMore = true; }
    setState(() => _loading = true);
    final token = await AuthStore.getToken();
    if (token == null) { return; }
    final items = await _api.getAlerts(token, page: _page);
    if (mounted) {
      setState(() {
        if (reset) { _alerts = items; } else { _alerts.addAll(items); }
        _hasMore = items.length == 20;
        _loading = false;
      });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Constants.bg,
      appBar: AppBar(
        backgroundColor: Constants.navy,
        foregroundColor: Colors.white,
        title: const Text('Active Alerts'),
      ),
      body: _loading && _alerts.isEmpty
          ? const Center(child: CircularProgressIndicator(color: Constants.navy))
          : RefreshIndicator(
              onRefresh: () => _load(reset: true),
              child: _alerts.isEmpty
                  ? const Center(child: Text('No active alerts at this time.',
                      style: TextStyle(color: Colors.grey)))
                  : ListView.builder(
                      padding: const EdgeInsets.all(12),
                      itemCount: _alerts.length + (_hasMore ? 1 : 0),
                      itemBuilder: (_, i) {
                        if (i == _alerts.length) {
                          return Padding(
                            padding: const EdgeInsets.all(16),
                            child: ElevatedButton(
                              onPressed: () { _page++; _load(); },
                              style: ElevatedButton.styleFrom(
                                backgroundColor: Constants.navy,
                                foregroundColor: Colors.white,
                              ),
                              child: const Text('Load More'),
                            ),
                          );
                        }
                        return _alertCard(_alerts[i]);
                      },
                    ),
            ),
    );
  }

  Widget _alertCard(AlertItem a) {
    return Card(
      margin: const EdgeInsets.only(bottom: 10),
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: Padding(
        padding: const EdgeInsets.all(14),
        child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
          Row(children: [
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
              decoration: BoxDecoration(
                color: a.severityColor.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(6),
                border: Border.all(color: a.severityColor, width: 1),
              ),
              child: Text(a.severity,
                style: TextStyle(color: a.severityColor,
                  fontSize: 11, fontWeight: FontWeight.bold)),
            ),
            const SizedBox(width: 8),
            Expanded(child: Text(a.category,
              style: const TextStyle(color: Colors.grey, fontSize: 12))),
          ]),
          const SizedBox(height: 8),
          Text(a.title, style: const TextStyle(
            fontSize: 15, fontWeight: FontWeight.bold, color: Constants.navy)),
          const SizedBox(height: 6),
          Text(a.message, style: const TextStyle(fontSize: 13, color: Color(0xFF4a5568))),
          const SizedBox(height: 8),
          Row(children: [
            const Icon(Icons.location_on_outlined, size: 14, color: Colors.grey),
            const SizedBox(width: 4),
            Text(a.region, style: const TextStyle(fontSize: 12, color: Colors.grey)),
            const Spacer(),
            const Icon(Icons.access_time, size: 14, color: Colors.grey),
            const SizedBox(width: 4),
            Text(a.publishedAt.length > 10 ? a.publishedAt.substring(0, 10) : a.publishedAt,
              style: const TextStyle(fontSize: 12, color: Colors.grey)),
          ]),
        ]),
      ),
    );
  }
}
