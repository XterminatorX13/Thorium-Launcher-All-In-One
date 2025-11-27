    Thorium Launcher — All-in-One
    =============================

    A lightweight, portable, and feature-rich launcher for Thorium Browser (and other Chromium-based browsers) written in C# (WinForms). 
    Designed to manage multiple profiles, handle advanced flags, and provide safe login modes without needing complex installations.

    ## Features

    ### 🚀 Profile Management
    - **Multiple Profiles**: Create, clone, and manage unlimited isolated profiles.
    - **Profile Manager**: View stats, **Rename**, and Delete profiles easily.
    - **Quick Actions**: Clone existing profiles or delete them with a single click.
    - **Portable Data**: All profiles are stored in the `Profiles/` subdirectory.

    ### 🛡️ Privacy & Security Modes
    - **Safe Mode (Login)**: Launches a temporary or persistent minimal session for Google Login.
    - **Hardened Mode**: Automatically detects and applies your advanced flags.
    - **Ephemeral Mode**: Launch a temporary session that deletes itself upon closing.

    ### ⚙️ Advanced Configuration
    - **Flag Editor**: Edit Chromium flags directly within the launcher.
    - **Desktop Shortcuts**: Export your config to a `.bat` file and automatically create a Desktop shortcut with the correct icon.
    - **Auto-Centering**: Automatically calculates and centers the browser window.

    ## How to Build (No SDK Required)

    You can compile this project using the native C# compiler included in Windows. No Visual Studio or .NET SDK installation is required.

    1. Open a Command Prompt (cmd) in the project folder.
    2. Run the build command:

      ```cmd
      C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe /target:winexe /out:ThoriumLauncher.exe /win32icon:"Umbra Puprpurea.ico" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:Microsoft.VisualBasic.dll /reference:System.Core.dll Program.cs
      ```

    3. Done! The `ThoriumLauncher.exe` will be created.

    ## Usage

    1. **Select Executable**: Point the launcher to your `thorium.exe`.
    2. **Choose Profile**: Select "Default" or create a new one.
    3. **Customize Flags**: Add your flags in the text box (or leave default).
    4. **Launch**: Click "LAUNCH" (or press Alt+L).

    ## Folder Structure & Session Data

    The launcher organizes all profile data in a clean, portable structure:

    ```
    thorium_all_in_one/
    ├── ThoriumLauncher.exe          # The launcher itself
    ├── Umbra Puprpurea.ico          # Custom icon (optional)
    ├── launcher.ini                 # Launcher settings (exe path, last profile)
    └── Profiles/                    # All profile data (PORTABLE!)
        ├── thorium-profile/         # Default profile folder
        │   ├── flags.txt            # Default profile flags
        │   ├── Cookies              # Session cookies
        │   ├── Login Data           # Saved passwords
        │   ├── History              # Browsing history
        │   ├── Local Storage/       # Website data
        │   └── ... (all Chromium user data)
        │
        └── thorium-profile-NAME/    # Custom profile folder
            ├── flags.txt            # Custom profile flags
            └── ... (isolated session data)
    ```

    **Where are my sessions stored?**
    - All browser data (cookies, history, passwords, cache) is stored in `Profiles/thorium-profile-[NAME]/`
    - Each profile is completely isolated with its own session data
    - The `flags.txt` file inside each profile folder contains the command-line flags
    - You can backup entire profiles by copying their folders

    **Portability:**
    - The entire `Profiles/` folder can be moved to another computer
    - Just copy the folder and update the executable path in the launcher

    ## Exporting Profiles

    When you click **"Export"**, the launcher saves a `.bat` file and offers to create shortcuts:

    ### Export Options:
    - **Yes** = Creates a **direct .lnk shortcut** (recommended!)
      - Points directly to `thorium.exe` with ALL flags in the Arguments field
      - Bypasses the 260-character Windows GUI limit
      - Supports up to **4096 characters** of flags programmatically
      - **Can be pinned to taskbar** without losing your settings!
      - File created: `Thorium - [ProfileName].lnk` on Desktop

    - **No** = Skip shortcuts (only saves the .bat file)

    - **Cancel** = Creates BOTH shortcuts:
      - Direct .lnk (for taskbar pinning)
      - .bat shortcut (for backward compatibility)

    ### Why the direct .lnk is better:
    ✅ **Taskbar pinning works!** Windows won't strip your flags  
    ✅ No CMD window flash (runs silently)  
    ✅ Supports very long flag lists (thousands of characters)  
    ✅ Custom icon support  
    ✅ Works exactly like a native Windows shortcut  

    **Note:** The `.bat` file is still useful for scripting or if you prefer batch files, but the `.lnk` shortcut is the best option for daily use and taskbar pinning.

    ## Native Thorium Profile Integration (NEW!)

    The launcher now **automatically detects** existing Thorium profiles in your browser's installation folder!

    ### How it works:
    1. **Auto-detection**: When you select a Thorium executable, the launcher scans the `User Data` folder
    2. **Native profiles appear**: Profiles from Thorium (like "Profile 1", "Profile 2", "Default") show up in the dropdown with a **[Native]** prefix
    3. **Add flags**: You can add custom flags to any native profile - they'll be saved in `User Data/[ProfileName]/flags.txt`
    4. **Seamless integration**: Launch native profiles with your custom flags, or use them as-is

    ### Example:
    ```
    Dropdown shows:
    - Default                    ← Launcher-managed profile
    - MyCustomProfile            ← Launcher-managed profile
    - [Native] Default           ← Thorium's native default profile
    - [Native] Profile 1         ← Thorium's native "Profile 1"
    - [Native] Profile 2         ← Thorium's native "Profile 2"
    ```

    ### Benefits:
    ✅ **No migration needed** - Use your existing Thorium profiles immediately  
    ✅ **Add flags to existing profiles** - Enhance native profiles with custom flags  
    ✅ **Unified management** - Manage both launcher and native profiles in one place  
    ✅ **Preserve browser data** - Keep all your cookies, history, and settings  

    **Note:** Native profiles are located in `[ThoriumDir]/User Data/[ProfileName]/` and are marked with `[Native]` to distinguish them from launcher-managed profiles in `Profiles/thorium-profile-[NAME]/`.

    ### How Native Profile Flags Work:

    **📝 When you select a native profile:**
    - If it has NO `flags.txt` file → Shows default flags as a starting template (not saved yet)
    - If it HAS `flags.txt` → Shows the saved flags from that file

    **💾 When you click "Save":**
    - Your flags are saved to: `User Data\[ProfileName]\flags.txt`
    - These flags will ALWAYS be used when launching this profile

    **🚀 When you click "Launch":**
    - Launcher reads the `flags.txt` (if it exists)
    - Builds command: `thorium.exe --user-data-dir="User Data\ProfileName" [your flags]`
    - Thorium opens with ALL profile data (cookies, passwords, history) + your custom flags

    **⚡ IMPORTANT - Flags Apply to the ENTIRE Thorium Instance:**
    When you launch `[Native] Default` with custom flags, those flags are applied to the **entire Thorium process**, not just one subprofile. This means:
    
    - If you have multiple browser profiles inside Thorium (like "Dallian", "Dalliance Support", "Personal", etc.)
    - ALL of them will use the same flags you set in `[Native] Default`
    - The flags affect the browser engine itself, not individual profiles
    
    **Example:**
    ```
    1. Select [Native] Default
    2. Add flag: --enable-features=VaapiVideoDecoder (hardware video acceleration)
    3. Save and Launch
    4. Thorium opens → You see the profile picker (Dallian, Personal, etc.)
    5. Choose ANY profile → ALL of them now have hardware acceleration! 🚀
    ```

    **💡 Think of it like this:**
    - **Launcher profiles** (`Default`, `MyProfile`) = Completely separate Thorium instances with their own flags
    - **Native profiles** (`[Native] Default`, `[Native] Profile 1`) = Your existing Thorium setup + custom flags applied to the whole browser
    - **Thorium's internal profiles** (Dallian, Personal, etc.) = Share the same flags from their parent native profile

    **Use cases:**
    - `[Native] Default` → Add performance flags to ALL your personal profiles (Dallian, etc.)
    - `[Native] Profile 1` → Separate work environment with different flags
    - `Default` (launcher) → Clean testing profile with its own flags

    Your browsing data stays untouched, but now you can supercharge your entire Thorium instance with custom flags!


    ## Requirements
    - Windows 10/11
    - .NET Framework 4.7.2 (pre-installed on most Windows systems)


    ## FAQ

    ### Q: Do command-line flags appear as "Enabled" in chrome://flags?
    **A:** No. Flags passed via command line (like in .bat files or shortcuts) do NOT appear as "Enabled" in the chrome://flags interface. That interface only controls preferences saved in the browser's `Local State` file.

    To verify if your flags are active, visit **chrome://version** and check the "Command Line" section. If your flags appear there, they are working.

    ### Q: Why does my browser look "default" even with flags?
    **A:** If the browser appears standard, it's because visual customization flags may not be present or the Thorium version changed behavior. Always verify active flags at **chrome://version**.

    ### Q: Why doesn't the exported .bat file close automatically?
    **A:** This has been fixed in the latest version. The generated .bat now includes an `exit` command to close the CMD window automatically after launching the browser.

    ### Q: Why isn't my browser window centered?
    **A:** The default "Hardened" profile uses `--start-maximized` instead of `--window-position`. If you want a centered window:
    - Add `--window-position=X,Y` manually to your flags, OR
    - Use the "Test Run" button → "Login Ephemeral: No" which automatically calculates centered positioning.

    ### Q: Why can't I login to Google with the default profile?
    **A:** The default "Hardened" profile contains privacy-focused flags like `--disable-background-networking` and anti-fingerprinting options that intentionally block Google login for privacy.

    **Solution:** Create a new "Standard" profile without these hardening flags if you need Google account access.

    ### Q: Are flags compatible across different Chromium browsers (Thorium, Ungoogled, etc.)?
    **A:** Flags depend on the Chromium version and specific fork. Each browser may support different flags, always check chrome://flags to see if the flag is supported or deprecated. Deprecated flags are simply ignored by the browser.

    ### Q: If I add flags to a native profile, will they be saved permanently?
    **A:** Yes! When you select a native profile (marked with `[Native]` prefix) and save flags, they are written to a `flags.txt` file inside that profile's folder. Every time you launch that profile through the launcher, those flags will be applied automatically.

    **Example:**
    1. Select `[Native] Profile 1`
    2. Edit flags and click "Save" → Creates `User Data\Profile 1\flags.txt`
    3. Click "Launch" → Thorium opens with Profile 1's data + your custom flags
    4. Next time you launch → Same flags are used automatically!

    **Important:** Your browsing data (cookies, passwords, history) is never touched. The launcher only adds performance/privacy flags on top of your existing profile.

    ### Q: How do I pin the launcher/shortcut to the Windows taskbar?
    **A:** When you export a .bat file, the launcher now offers to create a **direct .lnk shortcut** that solves the pinning problem!

    **The Problem:** When you pin a .bat shortcut to the taskbar, Windows creates a NEW shortcut pointing directly to `thorium.exe` WITHOUT your custom flags. This means your preferences won't load.

    **The Solution (NEW!):** 
    When exporting, choose **"Yes"** or **"Cancel"** to create a direct `.lnk` shortcut that:
    - Points directly to `thorium.exe` with ALL your flags as arguments
    - Bypasses the 260-character Windows GUI limit (supports up to 4096 characters programmatically)
    - **CAN be pinned to the taskbar** and will keep ALL your preferences!
    - Named "Thorium - [ProfileName].lnk" on your Desktop

    **Options when exporting:**
    - **Yes** = Create direct .lnk shortcut (recommended - can be pinned!)
    - **No** = Skip shortcuts
    - **Cancel** = Create BOTH .lnk AND .bat shortcuts

    **Alternative Solutions:**
    1. **Pin the Launcher itself**: Right-click `ThoriumLauncher.exe` → "Pin to taskbar" (then use it to launch your profiles)
    2. **Manual taskbar method**: Copy the .lnk shortcut to `%AppData%\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar`

    **Note:** The direct .lnk shortcut is the best option for taskbar pinning while keeping all your custom flags!
