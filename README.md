🔍 Missing References Finder

A Unity Editor tool to **find and optionally delete missing references and missing scripts** in Prefabs and ScriptableObjects.

---

✨ Features

✅ Scan all Prefabs for:
- Missing scripts  
- Missing Object references (e.g. materials, configs, sprites, addressables)

✅ Scan all ScriptableObjects for:
- Missing Object references

✅ Delete all missing scripts from Prefabs with one click

✅ Asynchronous scanning to avoid freezing Unity Editor

✅ Results UI with clickable paths to quickly locate broken assets

---

🚀 Installation

🔧 Option 1. Install via UPM (Git URL)

1. Open Unity Package Manager

2. Click + > Add package from git URL...

3. Paste: https://github.com/VD29/Unity-MissingReferencesFinder.git

📁 Option 2. Manual install

1. Copy MissingReferencesFinder.cs into your project under an Editor folder:

YourProject/
└── Assets/
    └── Editor/
        └── MissingReferencesFinder.cs

---

⚡ Usage

1. Open the window:  
   **Tools > Find Missing References**

2. Click:

- **Scan Prefabs** to scan all prefabs for missing references or scripts  
- **Scan ScriptableObjects** to scan all scriptable assets for missing references

3. Review the results displayed with counts. Click any listed asset path to locate it in the Project window.

4. Optional: Click **Delete Missing References in Prefabs** to automatically remove missing scripts from all affected prefabs.

---

✏️ Contributing

Pull requests and feature suggestions are welcome!

---

📬 Contact

Developed by VD29 / If you find this useful, please star ⭐ the repo and share!

