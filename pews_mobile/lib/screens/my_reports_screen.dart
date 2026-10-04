import 'package:flutter/material.dart';
import '../constants.dart';
import '../models/api_models.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';

/// Citizen-facing report list — shows only reports the signed-in user submitted,
/// read-only (no review/verify/approve/reject/delete — those live on the admin web portal).
class MyReportsScreen extends StatefulWidget {
  const MyReportsScreen({super.key});
  @override
  State<MyReportsScreen> createState() => _MyReportsScreenState();
}

class _MyReportsScreenState extends State<MyReportsScreen> {
  final _api = ApiService();
  List<MyReportItem> _reports = [];
  bool _loading  = true;
  int  _page     = 1;
  bool _hasMore  = true;

  @override
  void initState() {
    super.initState();
    _load(reset: true);
  }

  Future<void> _load({bool reset = false}) async {
    if (reset) { _page = 1; _hasMore = true; }
    setState(() => _loading = true);
    final token = await AuthStore.getToken();
    if (token == null) return;
    final items = await _api.getMyReports(token, page: _page);
    if (mounted) {
      setState(() {
        if (reset) { _reports = items; } else { _reports.addAll(items); }
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
        title: const Text('My Reports'),
      ),
      body: _loading && _reports.isEmpty
          ? const Center(child: CircularProgressIndicator(color: Constants.navy))
          : RefreshIndicator(
              onRefresh: () => _load(reset: true),
              child: _reports.isEmpty
                  ? ListView(children: const [
                      SizedBox(height: 120),
                      Center(child: Text("You haven't submitted any reports yet.",
                        style: TextStyle(color: Colors.grey))),
                    ])
                  : ListView.builder(
                      padding: const EdgeInsets.all(12),
                      itemCount: _reports.length + (_hasMore ? 1 : 0),
                      itemBuilder: (_, i) {
                        if (i == _reports.length) {
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
                        return _reportCard(_reports[i]);
                      },
                    ),
            ),
    );
  }

  Widget _reportCard(MyReportItem r) {
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
                color: r.severityColor.withValues(alpha: 0.15),
                borderRadius: BorderRadius.circular(6),
                border: Border.all(color: r.severityColor, width: 1),
              ),
              child: Text(r.severity,
                style: TextStyle(color: r.severityColor, fontSize: 11, fontWeight: FontWeight.bold)),
            ),
            const SizedBox(width: 8),
            Container(
              padding: const EdgeInsets.symmetric(horizontal: 8, vertical: 3),
              decoration: BoxDecoration(
                color: r.statusBg,
                borderRadius: BorderRadius.circular(6),
              ),
              child: Text(r.status,
                style: TextStyle(color: r.statusColor, fontSize: 11, fontWeight: FontWeight.bold)),
            ),
            if (r.isVerified) ...[
              const SizedBox(width: 6),
              const Icon(Icons.verified, size: 16, color: Color(0xFF065f46)),
            ],
            const Spacer(),
            if (r.imageCount > 0) ...[
              const Icon(Icons.image_outlined, size: 14, color: Colors.grey),
              const SizedBox(width: 2),
              Text('${r.imageCount}', style: const TextStyle(fontSize: 12, color: Colors.grey)),
            ],
          ]),
          const SizedBox(height: 8),
          Text(r.title, style: const TextStyle(
            fontSize: 15, fontWeight: FontWeight.bold, color: Constants.navy)),
          const SizedBox(height: 6),
          Row(children: [
            Text(r.disasterType, style: const TextStyle(fontSize: 12, color: Colors.grey)),
            if (r.region != null) ...[
              const Text(' · ', style: TextStyle(color: Colors.grey)),
              Text(r.region!, style: const TextStyle(fontSize: 12, color: Colors.grey)),
            ],
          ]),
          const SizedBox(height: 8),
          Row(children: [
            const Icon(Icons.access_time, size: 14, color: Colors.grey),
            const SizedBox(width: 4),
            Text(r.createdAt.length > 10 ? r.createdAt.substring(0, 10) : r.createdAt,
              style: const TextStyle(fontSize: 12, color: Colors.grey)),
          ]),
        ]),
      ),
    );
  }
}
