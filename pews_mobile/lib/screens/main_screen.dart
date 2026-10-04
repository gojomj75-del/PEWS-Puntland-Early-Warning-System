import 'package:flutter/material.dart';
import '../constants.dart';
import 'dashboard_screen.dart';
import 'submit_report_screen.dart';
import 'alerts_screen.dart';
import 'notifications_screen.dart';
import 'profile_screen.dart';

class MainScreen extends StatefulWidget {
  const MainScreen({super.key});
  @override
  State<MainScreen> createState() => _MainScreenState();
}

class _MainScreenState extends State<MainScreen> {
  int _index = 0;

  final _screens = const [
    DashboardScreen(),
    SubmitReportScreen(),
    AlertsScreen(),
    NotificationsScreen(),
    ProfileScreen(),
  ];

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      body: IndexedStack(index: _index, children: _screens),
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (i) => setState(() => _index = i),
        backgroundColor: Constants.navy,
        indicatorColor: Constants.gold,
        labelBehavior: NavigationDestinationLabelBehavior.alwaysShow,
        destinations: const [
          NavigationDestination(
            icon: Icon(Icons.dashboard_outlined, color: Colors.white70),
            selectedIcon: Icon(Icons.dashboard, color: Constants.navy),
            label: 'Dashboard',
          ),
          NavigationDestination(
            icon: Icon(Icons.add_circle_outline, color: Colors.white70),
            selectedIcon: Icon(Icons.add_circle, color: Constants.navy),
            label: 'Report',
          ),
          NavigationDestination(
            icon: Icon(Icons.warning_amber_outlined, color: Colors.white70),
            selectedIcon: Icon(Icons.warning_amber, color: Constants.navy),
            label: 'Alerts',
          ),
          NavigationDestination(
            icon: Icon(Icons.notifications_outlined, color: Colors.white70),
            selectedIcon: Icon(Icons.notifications, color: Constants.navy),
            label: 'Notifications',
          ),
          NavigationDestination(
            icon: Icon(Icons.person_outline, color: Colors.white70),
            selectedIcon: Icon(Icons.person, color: Constants.navy),
            label: 'Profile',
          ),
        ],
      ),
    );
  }
}
