import 'package:flutter/material.dart';
import 'package:image_picker/image_picker.dart';
import '../constants.dart';
import '../models/api_models.dart';
import '../services/api_service.dart';
import '../services/auth_store.dart';

class SubmitReportScreen extends StatefulWidget {
  const SubmitReportScreen({super.key});
  @override
  State<SubmitReportScreen> createState() => _SubmitReportScreenState();
}

class _SubmitReportScreenState extends State<SubmitReportScreen> {
  final _title       = TextEditingController();
  final _description = TextEditingController();
  final _location    = TextEditingController();
  final _api         = ApiService();
  final _picker      = ImagePicker();

  List<DisasterType> _types   = [];
  List<Region>       _regions = [];
  DisasterType?      _selType;
  Region?            _selRegion;
  int                _severity = 1;
  List<PickedImage>  _images   = [];
  bool   _loading = false;
  bool   _hasStatus = false;
  bool   _isSuccess = false;
  String _statusMsg = '';

  final _severities = ['Low', 'Medium', 'High', 'Critical'];

  @override
  void initState() {
    super.initState();
    _loadData();
  }

  Future<void> _loadData() async {
    final token = await AuthStore.getToken();
    if (token == null) { return; }
    final types   = await _api.getDisasterTypes(token);
    final regions = await _api.getRegions(token);
    if (mounted) {
      setState(() {
        _types   = types;
        _regions = regions;
        if (_types.isNotEmpty) { _selType = _types.first; }
      });
    }
  }

  Future<void> _pickImage(ImageSource source) async {
    if (_images.length >= 5) {
      _showStatus(false, 'Maximum 5 photos allowed.');
      return;
    }
    try {
      final xfile = await _picker.pickImage(source: source, imageQuality: 80);
      if (xfile != null) {
        final bytes = await xfile.readAsBytes();
        if (mounted) {
          setState(() => _images.add(PickedImage(bytes, xfile.name)));
        }
      }
    } catch (_) {}
  }

  Future<void> _submit() async {
    if (_title.text.isEmpty || _description.text.isEmpty || _selType == null) {
      _showStatus(false, 'Title, description, and disaster type are required.');
      return;
    }
    setState(() { _loading = true; _hasStatus = false; });
    final token = await AuthStore.getToken();
    if (token == null) return;

    final (ok, msg) = await _api.submitReport(
      token:          token,
      title:          _title.text.trim(),
      description:    _description.text.trim(),
      disasterTypeId: _selType!.id,
      severity:       _severity + 1,
      location:       _location.text.isEmpty ? null : _location.text.trim(),
      regionId:       _selRegion?.id,
      images:         _images,
    );

    if (mounted) {
      setState(() => _loading = false);
      _showStatus(ok, msg);
      if (ok) {
        _title.clear(); _description.clear(); _location.clear();
        setState(() {
          _images    = [];
          _severity  = 1;
          _selRegion = null;
          if (_types.isNotEmpty) _selType = _types.first;
        });
      }
    }
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
        title: const Text('Submit Report'),
      ),
      body: SingleChildScrollView(
        padding: const EdgeInsets.all(16),
        child: Column(children: [

          // Report info card
          _card('Report Information', [
            _label('Report Title *'),
            _textField(_title, 'e.g. Flash flood in Garowe'),
            const SizedBox(height: 14),
            _label('Description *'),
            TextField(
              controller: _description,
              maxLines: 4,
              decoration: _dec('What happened, current conditions, help needed…'),
            ),
            const SizedBox(height: 14),
            _label('Disaster Type *'),
            DropdownButtonFormField<DisasterType>(
              initialValue: _selType,
              items: _types.map((t) => DropdownMenuItem(value: t, child: Text(t.name))).toList(),
              onChanged: (v) => setState(() => _selType = v),
              decoration: _dec('Select type'),
            ),
            const SizedBox(height: 14),
            _label('Severity Level *'),
            DropdownButtonFormField<int>(
              initialValue: _severity,
              items: List.generate(_severities.length, (i) =>
                DropdownMenuItem(value: i, child: Text(_severities[i]))),
              onChanged: (v) => setState(() => _severity = v!),
              decoration: _dec('Select severity'),
            ),
          ]),

          const SizedBox(height: 12),

          // Location card
          _card('Location', [
            _label('Region'),
            DropdownButtonFormField<Region>(
              initialValue: _selRegion,
              items: [
                const DropdownMenuItem(value: null, child: Text('— Select Region —')),
                ..._regions.map((r) => DropdownMenuItem(value: r, child: Text(r.name))),
              ],
              onChanged: (v) => setState(() => _selRegion = v),
              decoration: _dec('Region'),
            ),
            const SizedBox(height: 14),
            _label('Specific Location / Landmark'),
            _textField(_location, 'Street, building, landmark…'),
          ]),

          const SizedBox(height: 12),

          // Photos card
          _card('Evidence Photos (max 5)', [
            Row(children: [
              Expanded(child: OutlinedButton.icon(
                icon: const Icon(Icons.camera_alt_outlined),
                label: const Text('Camera'),
                onPressed: () => _pickImage(ImageSource.camera),
                style: OutlinedButton.styleFrom(foregroundColor: Constants.navy,
                  side: const BorderSide(color: Constants.navy)),
              )),
              const SizedBox(width: 12),
              Expanded(child: OutlinedButton.icon(
                icon: const Icon(Icons.photo_library_outlined),
                label: const Text('Gallery'),
                onPressed: () => _pickImage(ImageSource.gallery),
                style: OutlinedButton.styleFrom(foregroundColor: Constants.navy,
                  side: const BorderSide(color: Constants.navy)),
              )),
            ]),
            const SizedBox(height: 12),
            if (_images.isEmpty)
              const Center(child: Text('No photos added',
                style: TextStyle(color: Colors.grey, fontSize: 13)))
            else
              SizedBox(
                height: 110,
                child: ListView.builder(
                  scrollDirection: Axis.horizontal,
                  itemCount: _images.length,
                  itemBuilder: (_, i) => Stack(
                    children: [
                      Container(
                        width: 100, height: 100, margin: const EdgeInsets.only(right: 8),
                        child: ClipRRect(
                          borderRadius: BorderRadius.circular(8),
                          child: Image.memory(_images[i].bytes, fit: BoxFit.cover),
                        ),
                      ),
                      Positioned(top: 4, right: 12,
                        child: GestureDetector(
                          onTap: () => setState(() => _images.removeAt(i)),
                          child: Container(
                            width: 22, height: 22,
                            decoration: BoxDecoration(
                              color: Colors.black54,
                              borderRadius: BorderRadius.circular(11),
                            ),
                            child: const Icon(Icons.close, color: Colors.white, size: 14),
                          ),
                        ),
                      ),
                    ],
                  ),
                ),
              ),
          ]),

          const SizedBox(height: 12),

          if (_hasStatus)
            Container(
              width: double.infinity,
              padding: const EdgeInsets.all(14),
              decoration: BoxDecoration(
                color: _isSuccess ? const Color(0xFFd1fae5) : const Color(0xFFfee2e2),
                borderRadius: BorderRadius.circular(10),
              ),
              child: Text(_statusMsg,
                style: TextStyle(color: _isSuccess
                  ? const Color(0xFF065f46) : const Color(0xFF991b1b))),
            ),

          const SizedBox(height: 12),

          SizedBox(
            width: double.infinity,
            height: 52,
            child: ElevatedButton(
              onPressed: _loading ? null : _submit,
              style: ElevatedButton.styleFrom(
                backgroundColor: Constants.navy,
                foregroundColor: Colors.white,
                shape: RoundedRectangleBorder(borderRadius: BorderRadius.circular(12)),
              ),
              child: _loading
                  ? const SizedBox(height: 22, width: 22,
                      child: CircularProgressIndicator(color: Colors.white, strokeWidth: 2))
                  : const Text('Submit Report',
                      style: TextStyle(fontSize: 16, fontWeight: FontWeight.bold)),
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
      {TextInputType? type}) =>
      TextField(controller: c, keyboardType: type, decoration: _dec(hint));

  InputDecoration _dec(String hint) => InputDecoration(
    hintText: hint,
    border: OutlineInputBorder(borderRadius: BorderRadius.circular(8)),
    focusedBorder: OutlineInputBorder(
      borderRadius: BorderRadius.circular(8),
      borderSide: const BorderSide(color: Constants.navy, width: 2),
    ),
    filled: true,
    fillColor: const Color(0xFFf7fafc),
    contentPadding: const EdgeInsets.symmetric(horizontal: 12, vertical: 10),
  );

  @override
  void dispose() {
    _title.dispose(); _description.dispose();
    _location.dispose();
    super.dispose();
  }
}
