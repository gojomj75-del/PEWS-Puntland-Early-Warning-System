import 'package:flutter/material.dart';
import 'constants.dart';
import 'screens/login_screen.dart';
import 'screens/main_screen.dart';
import 'services/auth_store.dart';

void main() {
  WidgetsFlutterBinding.ensureInitialized();
  runApp(const PewsApp());
}

class PewsApp extends StatelessWidget {
  const PewsApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'PEWS',
      debugShowCheckedModeBanner: false,
      theme: ThemeData(
        colorSchemeSeed: Constants.navy,
        useMaterial3: true,
        scaffoldBackgroundColor: Constants.bg,
      ),
      home: const _SplashRouter(),
    );
  }
}

class _SplashRouter extends StatefulWidget {
  const _SplashRouter();
  @override
  State<_SplashRouter> createState() => _SplashRouterState();
}

class _SplashRouterState extends State<_SplashRouter> {
  @override
  void initState() {
    super.initState();
    _check();
  }

  Future<void> _check() async {
    await Future.delayed(const Duration(milliseconds: 600));
    if (!mounted) return;
    final ok = await AuthStore.isLoggedIn();
    if (!mounted) return;
    Navigator.of(context).pushReplacement(MaterialPageRoute(
      builder: (_) => ok ? const MainScreen() : const LoginScreen(),
    ));
  }

  @override
  Widget build(BuildContext context) {
    return const Scaffold(
      backgroundColor: Constants.navy,
      body: Center(
        child: Column(
          mainAxisAlignment: MainAxisAlignment.center,
          children: [
            Icon(Icons.shield, color: Colors.white, size: 80),
            SizedBox(height: 16),
            Text('PEWS', style: TextStyle(
              color: Colors.white, fontSize: 32, fontWeight: FontWeight.bold)),
            SizedBox(height: 8),
            Text('Puntland Early Warning System',
              style: TextStyle(color: Color(0xFF8aabcc), fontSize: 14)),
            SizedBox(height: 40),
            CircularProgressIndicator(color: Color(0xFFe8a020)),
          ],
        ),
      ),
    );
  }
}
