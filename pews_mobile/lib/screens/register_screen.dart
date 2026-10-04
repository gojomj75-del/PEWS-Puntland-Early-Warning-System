import 'package:flutter/material.dart';
import '../constants.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';
import 'main_screen.dart';

class RegisterScreen extends StatefulWidget {
  const RegisterScreen({super.key});
  @override
  State<RegisterScreen> createState() => _RegisterScreenState();
}

class _RegisterScreenState extends State<RegisterScreen> {
  final _firstName = TextEditingController();
  final _lastName  = TextEditingController();
  final _email     = TextEditingController();
  final _password  = TextEditingController();
  final _confirm   = TextEditingController();
  final _phone     = TextEditingController();
  final _city      = TextEditingController();
  final _api       = ApiService();
  bool   _loading  = false;
  String _error    = '';
  bool   _obscure  = true;

  Future<void> _register() async {
    if (_password.text != _confirm.text) {
      setState(() => _error = 'Passwords do not match');
      return;
    }
    setState(() { _loading = true; _error = ''; });
    final (ok, msg, auth) = await _api.register(
      firstName: _firstName.text.trim(),
      lastName:  _lastName.text.trim(),
      email:     _email.text.trim(),
      password:  _password.text,
      phone:     _phone.text.isEmpty ? null : _phone.text.trim(),
      city:      _city.text.isEmpty  ? null : _city.text.trim(),
    );
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
      appBar: AppBar(
        backgroundColor: Constants.navy,
        foregroundColor: Colors.white,
        title: const Text('Create Account'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(20),
        child: Card(
          elevation: 4,
          shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(16)),
          child: Padding(
            padding: const EdgeInsets.all(24),
            child: Column(
              crossAxisAlignment: CrossAxisAlignment.stretch,
              children: [
                const Text('Register', style: TextStyle(
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

                Row(children: [
                  Expanded(child: _field(_firstName, 'First Name', Icons.person_outline)),
                  const SizedBox(width: 12),
                  Expanded(child: _field(_lastName, 'Last Name', Icons.person_outline)),
                ]),
                const SizedBox(height: 14),
                _field(_email, 'Email', Icons.email_outlined, type: TextInputType.emailAddress),
                const SizedBox(height: 14),
                _field(_phone, 'Phone (optional)', Icons.phone_outlined, type: TextInputType.phone),
                const SizedBox(height: 14),
                _field(_city, 'City (optional)', Icons.location_city_outlined),
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
                ),
                const SizedBox(height: 14),
                TextField(
                  controller: _confirm,
                  obscureText: _obscure,
                  decoration: _inputDec('Confirm Password', Icons.lock_outline),
                ),
                const SizedBox(height: 20),

                ElevatedButton(
                  onPressed: _loading ? null : _register,
                  style: ElevatedButton.styleFrom(
                    backgroundColor: Constants.navy,
                    foregroundColor: Colors.white,
                    padding: const EdgeInsets.symmetric(vertical: 14),
                    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                  ),
                  child: _loading
                      ? const SizedBox(height: 20, width: 20,
                          child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                      : const Text('Create Account', style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
                ),
              ],
            ),
          ),
        ),
      ),
    );
  }

  Widget _field(TextEditingController c, String label, IconData icon,
      {TextInputType? type}) =>
      TextField(controller: c, keyboardType: type, decoration: _inputDec(label, icon));

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
    _firstName.dispose(); _lastName.dispose(); _email.dispose();
    _password.dispose(); _confirm.dispose(); _phone.dispose(); _city.dispose();
    super.dispose();
  }
}
