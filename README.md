# OrgClockTray

OrgClockTray displays your currently running clock time in a tray
icon in Windows' Taskbar.

If you use org-mode to clock your work time, but your work requires you to
use tools other than emacs, you may find yourself switching back and
forth to check the time, or make sure that you've clocked-in at
all and didn't clock-out by mistake.

If this is the case, OrgClockTray is for you.

## Screenshots

Icon displayed when the clock is running:

![Icon displayed when the clock's running](https://github.com/schmendrik/OrgClockTray/blob/master/Resources/Screenshot2.png
 "Active clock")

Icon displayed when idle:

![Icon displayed when idle](https://github.com/schmendrik/OrgClockTray/blob/master/Resources/Screenshot1.png
 "No active clock")

## Getting Started

1. In order for OrgClockTray to know your clock time, you need to add
   this to your emacs' init file: ```lisp
(defun current-clock-time-to-file ()
  (interactive)
  (with-temp-file "~/.emacs.d/.task"
    (if (org-clocking-p)
        (insert (org-clock-get-clock-string))
        (insert ""))))
(run-with-timer 1 60 'current-clock-time-to-file)
(add-hook 'org-clock-in-hook 'current-clock-time-to-file)
(add-hook 'org-clock-out-hook 'current-clock-time-to-file)
```
2. Download the OrgClockTray release [here](https://github.com/schmendrik/OrgClockTray/releases).
3. Put a shortcut to OrgClockTray into your Startup folder (optional).
4. Test: clock in and watch OrgClockTray display the time in a tray
   icon.

Note: By default, OrgClockTray reads from a file
named '.task' that is located in your .emacs.d directory. You can
modify the file path in the lisp code to export the time to another
file path, which you'll need to pass to OrgClockTray as a command line
argument.

## Time Format

Since the space within a 16x16 pixel icon is limited, the total time
is converted into hours, which is displayed as a decimal number.
the hour.

If the hours exceed single-digit, they'll be displayed as an integer
number to conserve enough space for double and triple-digit numbers.
