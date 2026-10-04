import 'package:flutter/material.dart';
import '../constants.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';
import 'register_screen.dart';
import 'main_screen.dart';

class LoginScreen extends StatefulWidget {
  const LoginScreen({super.key});
  @override
  State<LoginScreen> createState() => _LoginScreenState();
}

class _LoginScreenState extends State<LoginScreen> {
  final _email    = TextEditingController();
  final _password = TextEditingController();
  final _api      = ApiService();
  bool   _loading  = false;
  String _error    = '';
  bool   _obscure  = true;

  Future<void> _login() async {
    setState(() { _loading = true; _error = ''; });
    final (ok, msg, auth) = await _api.login(_email.text.trim(), _password.text);
    if (!mounted) return;
    if (ok && auth != null) {
      await AuthStore.save(auth);
      if (!mounted) return;
      Navigator.of(context).pushReplacement(
        MaterialPageRoute(builder: (_) => const MainScreen()));
    } else {
      setState(() { _error = msg; _loading = false; });
    }
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Constants.bg,
      body: SafeArea(
        child: Center(
          child: SingleChildScrollView(
            padding: const EdgeInsets.all(24),
            child: Column(
              children: [
                // Logo
                Container(
                  width: 80, height: 80,
                  decoration: BoxDecoration(
                    color: Constants.navy,
                    borderRadius: BorderRadius.circular(20),
                  ),
                  child: const Icon(Icons.shield, color: Colors.white, size: 44),
                ),
                const SizedBox(height: 16),
                const Text('PEWS', style: TextStyle(
                  fontSize: 28, fontWeight: FontWeight.bold, color: Constants.navy)),
                const Text('Puntland Early Warning System',
                  style: TextStyle(fontSize: 13, color: Colors.grey)),
                const SizedBox(height: 32),

                // Card
                Card(
                  elevation: 4,
                  shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
                  child: Padding(
                    padding: const EdgeInsets.all(24),
                    child: Column(
                      crossAxisAlignment: CrossAxisAlignment.stretch,
                      children: [
                        const Text('Sign In', style: TextStyle(
                          fontSize: 18, fontWeight: FontWeight.bold, color: Constants.navy)),
                        const SizedBox(height: 20),

                        if (_error.isNotEmpty) ...[
                          Container(
                            padding: const EdgeInsets.all(12),
                            decoration: BoxDecoration(
                              color: const Color(0xFFfee2e2),
                              borderRadius: BorderRadius.circular(8),
                            ),
                            child: Text(_error, style: const TextStyle(color: Color(0xFF991b1b))),
                          ),
                          const SizedBox(height: 12),
                        ],

                        TextField(
                          controller: _email,
                          keyboardType: TextInputType.emailAddress,
                          decoration: _inputDec('Email address', Icons.email_outlined),
                        ),
                        const SizedBox(height: 14),
                        TextField(
                          controller: _password,
                          obscureText: _obscure,
                          decoration: _inputDec('Password', Icons.lock_outline).copyWith(
                            suffixIcon: IconButton(
                              icon: Icon(_obscure ? Icons.visibility_off : Icons.visibility),
                              onPressed: () => setState(() => _obscure = !_obscure),
                            ),
                          ),
                          onSubmitted: (_) => _login(),
                        ),
                        const SizedBox(height: 20),

                        ElevatedButton(
                          onPressed: _loading ? null : _login,
                          style: ElevatedButton.styleFrom(
                            backgroundColor: Constants.navy,
                            foregroundColor: Colors.white,
                            padding: const EdgeInsets.symmetric(vertical: 14),
                            shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                          ),
                          child: _loading
                              ? const SizedBox(height: 20, width: 20,
                                  child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                              : const Text('Sign In', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                        ),
                      ],
                    ),
                  ),
                ),

                const SizedBox(height: 16),
                Row(mainAxisAlignment: MainAxisAlignment.center, children: [
                  const Text("Don't have an account? "),
                  GestureDetector(
                    onTap: () => Navigator.of(context).push(
                      MaterialPageRoute(builder: (_) => const RegisterScreen())),
                    child: const Text('Register', style: TextStyle(
                      color: Constants.navy, fontWeight: FontWeight.bold)),
                  ),
                ]),
              ],
            ),
          ),
        ),
      ),
    );
  }

  InputDecoration _inputDec(String label, IconData icon) => InputDecoration(
    labelText: label,
    prefixIcon: Icon(icon, color: Constants.navy),
    border: OutlineInputBorder(borderRadius: BorderRadius.circular(10)),
    focusedBorder: OutlineInputBorder(
      borderRadius: BorderRadius.circular(10),
      borderSide: const BorderSide(color: Constants.navy, width: 2),
    ),
    filled: true,
    fillColor: const Color(0xFFf7fafc),
  );

  @override
  void dispose() {
    _email.dispose(); _password.dispose(); super.dispose();
  }
}
