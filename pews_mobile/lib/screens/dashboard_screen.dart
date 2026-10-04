import 'package:flutter/material.dart';
import '../constants.dart';
import '../models/api_models.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';
import 'login_screen.dart';
import 'my_reports_screen.dart';

class DashboardScreen extends StatefulWidget {
  const DashboardScreen({super.key});
  @override
  State<DashboardScreen> createState() => _DashboardScreenState();
}

class _DashboardScreenState extends State<DashboardScreen> {
  final _api = ApiService();
  DashboardStats? _stats;
  String _fullName = '';
  String _role     = '';
  bool   _loading  = true;

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final token = await AuthStore.getToken();
    final name  = await AuthStore.getFullName();
    final role  = await AuthStore.getRole();
    if (!mounted) return;
    setState(() { _fullName = name ?? ''; _role = role ?? ''; });
    if (token != null) {
      final stats = await _api.getStats(token);
      if (mounted) setState(() { _stats = stats; _loading = false; });
    }
  }

  void _openMyReports() {
    Navigator.of(context).push(MaterialPageRoute(builder: (_) => const MyReportsScreen()));
  }

  Future<void> _logout() async {
    await AuthStore.clear();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()), (_) => false);
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Constants.bg,
      appBar: AppBar(
        backgroundColor: Constants.navy,
        foregroundColor: Colors.white,
        title: const Text('Dashboard'),
        actions: [
          IconButton(icon: const Icon(Icons.logout), onPressed: _logout, tooltip: 'Logout'),
        ],
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: Constants.navy))
          : RefreshIndicator(
              onRefresh: _load,
              child: ListView(
                padding: const EdgeInsets.all(16),
                children: [
                  // Welcome card
                  Card(
                    color: Constants.navy,
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                    child: Padding(
                      padding: const EdgeInsets.all(20),
                      child: Row(children: [
                        const Icon(Icons.shield, color: Colors.white, size: 40),
                        const SizedBox(width: 16),
                        Expanded(child: Column(
                          crossAxisAlignment: CrossAxisAlignment.start,
                          children: [
                            Text('Welcome, $_fullName',
                              style: const TextStyle(color: Colors.white,
                                fontSize: 16, fontWeight: FontWeight.bold)),
                            Text(_role, style: const TextStyle(
                              color: Color(0xFF8aabcc), fontSize: 13)),
                          ],
                        )),
                      ]),
                    ),
                  ),
                  const SizedBox(height: 16),

                  const Text('Overview', style: TextStyle(
                    fontSize: 16, fontWeight: FontWeight.bold, color: Constants.navy)),
                  const SizedBox(height: 12),

                  if (_stats != null) ...[
                    GridView.count(
                      crossAxisCount: 2,
                      shrinkWrap: true,
                      physics: const NeverScrollableScrollPhysics(),
                      crossAxisSpacing: 12,
                      mainAxisSpacing: 12,
                      childAspectRatio: 1.4,
                      children: [
                        _statCard('My Reports',       _stats!.totalReports,        Icons.description_outlined,    const Color(0xFF1d4ed8), onTap: _openMyReports),
                        _statCard('Active Alerts',    _stats!.activeAlerts,        Icons.warning_amber_outlined,  const Color(0xFFb45309)),
                        _statCard('Pending',          _stats!.pendingReports,      Icons.hourglass_empty_outlined, const Color(0xFF6d28d9), onTap: _openMyReports),
                        _statCard('Verified',         _stats!.verifiedReports,     Icons.check_circle_outline,    const Color(0xFF065f46), onTap: _openMyReports),
                      ],
                    ),
                    const SizedBox(height: 12),
                    if (_stats!.unreadNotifications > 0)
                      Card(
                        color: const Color(0xFFfef3c7),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
                        child: Padding(
                          padding: const EdgeInsets.all(14),
                          child: Row(children: [
                            const Icon(Icons.notifications_active, color: Color(0xFFb45309)),
                            const SizedBox(width: 10),
                            Text('${_stats!.unreadNotifications} unread notification(s)',
                              style: const TextStyle(color: Color(0xFF92400e), fontWeight: FontWeight.w600)),
                          ]),
                        ),
                      ),
                  ] else
                    const Center(child: Text('Could not load stats. Pull to refresh.')),
                ],
              ),
            ),
    );
  }

  Widget _statCard(String label, int value, IconData icon, Color color, {VoidCallback? onTap}) {
    return Card(
      elevation: 2,
      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
      child: InkWell(
        borderRadius: BorderRadius.circular(12),
        onTap: onTap,
        child: Padding(
          padding: const EdgeInsets.all(14),
          child: Column(
            crossAxisAlignment: CrossAxisAlignment.start,
            mainAxisAlignment: MainAxisAlignment.spaceBetween,
            children: [
              Icon(icon, color: color, size: 28),
              Column(
                crossAxisAlignment: CrossAxisAlignment.start,
                children: [
                  Text('$value', style: TextStyle(
                    fontSize: 26, fontWeight: FontWeight.bold, color: color)),
                  Text(label, style: const TextStyle(fontSize: 12, color: Colors.grey)),
                ],
              ),
            ],
          ),
        ),
      ),
    );
  }
}
