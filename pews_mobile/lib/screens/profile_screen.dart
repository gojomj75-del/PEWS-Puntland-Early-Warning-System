import 'package:flutter/material.dart';
import '../constants.dart';
import '../models/api_models.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';
import 'login_screen.dart';

class ProfileScreen extends StatefulWidget {
  const ProfileScreen({super.key});
  @override
  State<ProfileScreen> createState() => _ProfileScreenState();
}

class _ProfileScreenState extends State<ProfileScreen> {
  final _api = ApiService();

  final _firstName = TextEditingController();
  final _lastName  = TextEditingController();
  final _phone     = TextEditingController();
  final _address   = TextEditingController();
  final _city      = TextEditingController();
  final _region    = TextEditingController();

  final _currentPassword = TextEditingController();
  final _newPassword     = TextEditingController();
  final _confirmPassword = TextEditingController();

  Profile? _profile;
  bool _loading = true;
  bool _saving  = false;
  bool _changingPassword = false;

  bool   _hasStatus = false;
  bool   _isSuccess = false;
  String _statusMsg = '';

  @override
  void initState() {
    super.initState();
    _load();
  }

  Future<void> _load() async {
    final token = await AuthStore.getToken();
    if (token == null) return;
    final profile = await _api.getProfile(token);
    if (mounted) {
      setState(() {
        _profile = profile;
        _loading = false;
        if (profile != null) {
          _firstName.text = profile.firstName;
          _lastName.text  = profile.lastName;
          _phone.text     = profile.phoneNumber ?? '';
          _address.text   = profile.address ?? '';
          _city.text      = profile.city ?? '';
          _region.text    = profile.region ?? '';
        }
      });
    }
  }

  Future<void> _saveProfile() async {
    if (_firstName.text.trim().isEmpty || _lastName.text.trim().isEmpty) {
      _showStatus(false, 'First name and last name are required.');
      return;
    }
    setState(() { _saving = true; _hasStatus = false; });
    final token = await AuthStore.getToken();
    if (token == null) return;

    final (ok, msg) = await _api.updateProfile(
      token: token,
      firstName: _firstName.text.trim(),
      lastName: _lastName.text.trim(),
      phoneNumber: _phone.text.trim().isEmpty ? null : _phone.text.trim(),
      address: _address.text.trim().isEmpty ? null : _address.text.trim(),
      city: _city.text.trim().isEmpty ? null : _city.text.trim(),
      region: _region.text.trim().isEmpty ? null : _region.text.trim(),
    );

    if (mounted) {
      setState(() => _saving = false);
      _showStatus(ok, msg);
    }
  }

  Future<void> _submitPasswordChange() async {
    if (_newPassword.text != _confirmPassword.text) {
      _showStatus(false, 'New password and confirmation do not match.');
      return;
    }
    if (_newPassword.text.length < 8) {
      _showStatus(false, 'New password must be at least 8 characters.');
      return;
    }
    setState(() { _changingPassword = true; _hasStatus = false; });
    final token = await AuthStore.getToken();
    if (token == null) return;

    final (ok, msg) = await _api.changePassword(
      token: token,
      currentPassword: _currentPassword.text,
      newPassword: _newPassword.text,
    );

    if (mounted) {
      setState(() => _changingPassword = false);
      _showStatus(ok, msg);
      if (ok) {
        _currentPassword.clear(); _newPassword.clear(); _confirmPassword.clear();
      }
    }
  }

  Future<void> _logout() async {
    await AuthStore.clear();
    if (!mounted) return;
    Navigator.of(context).pushAndRemoveUntil(
      MaterialPageRoute(builder: (_) => const LoginScreen()), (_) => false);
  }

  void _showStatus(bool success, String msg) => setState(() {
    _hasStatus = true; _isSuccess = success; _statusMsg = msg;
  });

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      backgroundColor: Constants.bg,
      appBar: AppBar(
        backgroundColor: Constants.navy,
        foregroundColor: Colors.white,
        title: const Text('My Profile'),
      ),
      body: _loading
          ? const Center(child: CircularProgressIndicator(color: Constants.navy))
          : SingleChildScrollView(
              padding: const EdgeInsets.all(16),
              child: Column(children: [
                // Avatar header
                CircleAvatar(
                  radius: 38,
                  backgroundColor: Constants.navy,
                  child: Text(
                    _profile?.firstName.isNotEmpty == true
                        ? _profile!.firstName.substring(0, 1).toUpperCase() : '?',
                    style: const TextStyle(fontSize: 32, color: Colors.white, fontWeight: FontWeight.bold),
                  ),
                ),
                const SizedBox(height: 10),
                Text(_profile?.email ?? '', style: const TextStyle(color: Colors.grey, fontSize: 13)),
                const SizedBox(height: 16),

                if (_hasStatus)
                  Container(
                    width: double.infinity,
                    margin: const EdgeInsets.only(bottom: 12),
                    padding: const EdgeInsets.all(14),
                    decoration: BoxDecoration(
                      color: _isSuccess ? const Color(0xFFd1fae5) : const Color(0xFFfee2e2),
                      borderRadius: BorderRadius.circular(10),
                    ),
                    child: Text(_statusMsg,
                      style: TextStyle(color: _isSuccess
                        ? const Color(0xFF065f46) : const Color(0xFF991b1b))),
                  ),

                _card('Personal Information', [
                  _label('First Name'),
                  _textField(_firstName, 'First name'),
                  const SizedBox(height: 14),
                  _label('Last Name'),
                  _textField(_lastName, 'Last name'),
                  const SizedBox(height: 14),
                  _label('Phone Number'),
                  _textField(_phone, 'Phone number', type: TextInputType.phone),
                  const SizedBox(height: 14),
                  _label('Address'),
                  _textField(_address, 'Address'),
                  const SizedBox(height: 14),
                  _label('City'),
                  _textField(_city, 'City'),
                  const SizedBox(height: 14),
                  _label('Region'),
                  _textField(_region, 'Region'),
                  const SizedBox(height: 18),
                  SizedBox(
                    width: double.infinity,
                    child: ElevatedButton(
                      onPressed: _saving ? null : _saveProfile,
                      style: ElevatedButton.styleFrom(
                        backgroundColor: Constants.navy, foregroundColor: Colors.white,
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      child: _saving
                          ? const SizedBox(height: 20, width: 20,
                              child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                          : const Text('Save Changes', style: TextStyle(fontWeight: FontWeight.bold)),
                    ),
                  ),
                ]),

                const SizedBox(height: 12),

                _card('Change Password', [
                  _label('Current Password'),
                  _textField(_currentPassword, 'Current password', obscure: true),
                  const SizedBox(height: 14),
                  _label('New Password'),
                  _textField(_newPassword, 'New password', obscure: true),
                  const SizedBox(height: 14),
                  _label('Confirm New Password'),
                  _textField(_confirmPassword, 'Confirm new password', obscure: true),
                  const SizedBox(height: 18),
                  SizedBox(
                    width: double.infinity,
                    child: OutlinedButton(
                      onPressed: _changingPassword ? null : _submitPasswordChange,
                      style: OutlinedButton.styleFrom(
                        foregroundColor: Constants.navy,
                        side: const BorderSide(color: Constants.navy),
                        padding: const EdgeInsets.symmetric(vertical: 14),
                        shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                      ),
                      child: _changingPassword
                          ? const SizedBox(height: 20, width: 20,
                              child: CircularProgressIndicator(color: Constants.navy, strokeWidth: 2))
                          : const Text('Update Password', style: TextStyle(fontWeight: FontWeight.bold)),
                    ),
                  ),
                ]),

                const SizedBox(height: 20),

                SizedBox(
                  width: double.infinity,
                  child: OutlinedButton.icon(
                    onPressed: _logout,
                    icon: const Icon(Icons.logout, color: Colors.red),
                    label: const Text('Sign Out', style: TextStyle(color: Colors.red, fontWeight: FontWeight.bold)),
                    style: OutlinedButton.styleFrom(
                      side: const BorderSide(color: Colors.red),
                      padding: const EdgeInsets.symmetric(vertical: 14),
                      shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(10)),
                    ),
                  ),
                ),
                const SizedBox(height: 20),
              ]),
            ),
    );
  }

  Widget _card(String title, List<Widget> children) => Card(
    elevation: 3,
    shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(14)),
    child: Padding(
      padding: const EdgeInsets.all(18),
      child: Column(crossAxisAlignment: CrossAxisAlignment.start, children: [
        Text(title, style: const TextStyle(
          fontSize: 15, fontWeight: FontWeight.bold, color: Constants.navy)),
        const SizedBox(height: 14),
        ...children,
      ]),
    ),
  );

  Widget _label(String text) => Padding(
    padding: const EdgeInsets.only(bottom: 6),
    child: Text(text, style: const TextStyle(
      fontSize: 12, fontWeight: FontWeight.bold, color: Color(0xFF4a5568))),
  );

  Widget _textField(TextEditingController c, String hint,
      {TextInputType? type, bool obscure = false}) =>
      TextField(
        controller: c,
        keyboardType: type,
        obscureText: obscure,
        decoration: InputDecoration(
          hintText: hint,
          border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
          focusedBorder: OutlineInputBorder(
            borderRadius: BorderRadius.circular(8),
            borderSide: const BorderSide(color: Constants.navy, width: 2),
          ),
          filled: true,
          fillColor: const Color(0xFFf7fafc),
          contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
        ),
      );

  @override
  void dispose() {
    _firstName.dispose(); _lastName.dispose(); _phone.dispose();
    _address.dispose(); _city.dispose(); _region.dispose();
    _currentPassword.dispose(); _newPassword.dispose(); _confirmPassword.dispose();
    super.dispose();
  }
}
