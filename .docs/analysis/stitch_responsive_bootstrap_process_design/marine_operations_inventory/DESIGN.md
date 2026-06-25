---
name: Marine Operations & Inventory
colors:
  surface: '#f8f9fa'
  surface-dim: '#d9dadb'
  surface-bright: '#f8f9fa'
  surface-container-lowest: '#ffffff'
  surface-container-low: '#f3f4f5'
  surface-container: '#edeeef'
  surface-container-high: '#e7e8e9'
  surface-container-highest: '#e1e3e4'
  on-surface: '#191c1d'
  on-surface-variant: '#434654'
  inverse-surface: '#2e3132'
  inverse-on-surface: '#f0f1f2'
  outline: '#737685'
  outline-variant: '#c3c6d6'
  surface-tint: '#0f55d1'
  primary: '#0753cf'
  on-primary: '#ffffff'
  primary-container: '#366de9'
  on-primary-container: '#fefcff'
  inverse-primary: '#b3c5ff'
  secondary: '#1b6d24'
  on-secondary: '#ffffff'
  secondary-container: '#a0f399'
  on-secondary-container: '#217128'
  tertiary: '#555d64'
  on-tertiary: '#ffffff'
  tertiary-container: '#6d757d'
  on-tertiary-container: '#fcfcff'
  error: '#ba1a1a'
  on-error: '#ffffff'
  error-container: '#ffdad6'
  on-error-container: '#93000a'
  primary-fixed: '#dae1ff'
  primary-fixed-dim: '#b3c5ff'
  on-primary-fixed: '#001849'
  on-primary-fixed-variant: '#003fa4'
  secondary-fixed: '#a3f69c'
  secondary-fixed-dim: '#88d982'
  on-secondary-fixed: '#002204'
  on-secondary-fixed-variant: '#005312'
  tertiary-fixed: '#dbe3ec'
  tertiary-fixed-dim: '#bfc7d0'
  on-tertiary-fixed: '#151c23'
  on-tertiary-fixed-variant: '#40484f'
  background: '#f8f9fa'
  on-background: '#191c1d'
  surface-variant: '#e1e3e4'
typography:
  headline-lg:
    fontFamily: Hanken Grotesk
    fontSize: 28px
    fontWeight: '500'
    lineHeight: '1.2'
    letterSpacing: -0.02em
  headline-md:
    fontFamily: Hanken Grotesk
    fontSize: 22px
    fontWeight: '500'
    lineHeight: '1.3'
  body-md:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1.5'
  label-sm:
    fontFamily: Inter
    fontSize: 12px
    fontWeight: '600'
    lineHeight: '1'
  nav-link:
    fontFamily: Inter
    fontSize: 14px
    fontWeight: '400'
    lineHeight: '1'
rounded:
  sm: 0.125rem
  DEFAULT: 0.25rem
  md: 0.375rem
  lg: 0.5rem
  xl: 0.75rem
  full: 9999px
spacing:
  container-padding: 1.5rem
  gutter: 1rem
  element-gap: 0.75rem
  section-margin: 2rem
---

## Brand & Style
The brand personality is professional, utility-driven, and systematic, designed for high-stakes maritime environments where clarity is paramount. The design system prioritizes a **Corporate / Modern** style that leans heavily into functional minimalism. 

The aesthetic is characterized by a "clean and business-like" atmosphere, using a cool-toned palette to evoke a sense of calm and precision. It focuses on task completion—specifically for scanning and inventory workflows—by removing visual noise and using clear, high-contrast action triggers. The target audience includes maritime personnel who require an interface that remains legible in varied lighting conditions and under time pressure.

## Colors
The palette is built on a foundation of neutral grays and cool blues, following a structured functional logic.
- **Primary Blue:** Reserved for the most important "final" actions, such as 'Opslaan' (Save).
- **Secondary Green:** Used specifically for "Add" or "Confirm" actions related to stock management and successful scanning outcomes.
- **Background Tones:** A tiered system of light-blue-grays (`#f8f9fa`) and whites to separate the navigation, page headers, and form containers.
- **Neutral Accents:** Medium grays (`#868e96`) are used for secondary UI elements like "Close" buttons and placeholder states to maintain a clear visual hierarchy.

## Typography
The system uses a modern sans-serif stack to ensure maximum legibility for data-heavy screens and mobile scanning views. **Hanken Grotesk** provides a clean, contemporary feel for headings, while **Inter** handles the functional body and form data.

Typography is scaled to be compact but accessible. Form labels use a smaller, bolded Inter to differentiate themselves clearly from user input. For mobile-specific scanning views, body-md is the minimum size to ensure readability on handheld devices.

## Layout & Spacing
This design system utilizes a **Fixed Grid** philosophy based on standard Bootstrap-aligned breakpoints. Content is typically housed within a 12-column container to ensure alignment across complex forms.

- **Mobile (Scanning Flow):** Transitions to a single-column stacked layout. Padding is increased on touch targets to accommodate field work.
- **Desktop (Management/Log):** Uses a multi-column approach where inputs are grouped logically (e.g., Departure and Arrival side-by-side).
- **Rhythm:** A base 4px/8px spacing system is used. Standard vertical rhythm between form groups is 1rem (16px) to maintain a dense but readable business UI.

## Elevation & Depth
Depth is created primarily through **Tonal Layers** rather than heavy shadows, keeping the UI "flat" and performant. 
- **Level 0 (Background):** Light gray base layer.
- **Level 1 (Cards/Containers):** Pure white surfaces with a 1px solid border (`#dee2e6`).
- **Level 2 (Modals):** High-contrast overlays with a soft ambient shadow (0px 4px 12px rgba(0,0,0,0.1)) to focus attention on scanning inputs or product lookups.
- **Interactive States:** Buttons use a subtle darkening of their fill color on hover, rather than an elevation change, to maintain the professional, grounded feel.

## Shapes
The shape language is conservative and geometric, utilizing a "Soft" (0.25rem) radius for standard elements like buttons and input fields. This provides a modern touch without sacrificing the serious, professional nature of the tool.

Larger components like cards and modal windows use `rounded-lg` (0.5rem) to provide a soft container for the dense data within.

## Components
- **Buttons:** Primary buttons ('Opslaan') are solid Blue. Secondary 'Add' buttons are solid Green with a leading '+' icon. Navigation 'Back' buttons use a ghost style with a subtle border.
- **Input Fields:** Standardized white backgrounds with `#ced4da` borders. Active/Focus state uses the Primary Blue for the border.
- **Scanning Interface:** Features large, high-contrast text fields and immediate visual feedback (Green for success, Red for error) to accommodate the quick pace of barcode/QR scanning.
- **Tables (Logbook):** Zebra-striping is avoided; instead, use thin horizontal dividers and high-contrast header text. Action icons (Edit/Delete) are contained in subtle gray bordered squares.
- **Status Chips:** Small, rounded pills used for location tags or stock status, utilizing the tertiary gray or muted secondary colors.
- **Modals:** Centered, white containers with a clear 'Close' action in the top right. Primary focus is placed on a single large input for searching or code entry.