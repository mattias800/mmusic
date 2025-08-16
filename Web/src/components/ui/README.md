# Beautiful UI Component Library 🎨

A collection of reusable, beautiful UI components designed for modern web applications with a dark theme and glass morphism effects.

## 🚀 Quick Start

```tsx
import { 
  PageLayout, 
  PageHeader, 
  GlassCard, 
  StatusCard, 
  StatusGrid, 
  GradientButton, 
  InfoSection 
} from "@/components/ui";
```

## 📱 Components

### PageLayout
Provides the dark gradient background and container structure for your pages.

```tsx
<PageLayout>
  {/* Your page content */}
</PageLayout>
```

**Props:**
- `className?: string` - Additional CSS classes
- `containerClassName?: string` - Additional classes for the container

### PageHeader
Beautiful header section with gradient avatar, title, and subtitle.

```tsx
<PageHeader 
  icon={User} 
  title="User Profile" 
  subtitle="Manage your account settings"
  avatarSize="lg" // "sm" | "md" | "lg"
/>
```

**Props:**
- `icon: LucideIcon` - Icon to display in the avatar
- `title: string` - Main heading text
- `subtitle?: string` - Optional subtitle
- `avatarSize?: "sm" | "md" | "lg"` - Size of the avatar
- `className?: string` - Additional CSS classes

### GlassCard
Glass morphism card with backdrop blur effects and optional header.

```tsx
<GlassCard 
  title="Card Title" 
  icon={Settings} 
  iconBgColor="bg-blue-500/20"
  padding="lg" // "sm" | "md" | "lg"
>
  {/* Card content */}
</GlassCard>
```

**Props:**
- `title?: string` - Optional card title
- `icon?: LucideIcon` - Optional icon for the header
- `iconBgColor?: string` - Background color for the icon (default: "bg-blue-500/20")
- `padding?: "sm" | "md" | "lg"` - Card padding size
- `className?: string` - Additional CSS classes

### StatusCard
Individual status/information card for displaying key-value pairs.

```tsx
<StatusCard 
  label="Username" 
  value="john_doe" 
  icon={User}
/>
```

**Props:**
- `label: string` - Label text
- `value: React.ReactNode` - Value to display
- `icon?: LucideIcon` - Optional icon
- `className?: string` - Additional CSS classes

### StatusGrid
Grid layout for organizing multiple StatusCard components.

```tsx
<StatusGrid columns={2}>
  <StatusCard label="Username" value="john_doe" />
  <StatusCard label="Email" value="john@example.com" />
</StatusGrid>
```

**Props:**
- `columns?: 1 | 2 | 3 | 4` - Number of columns (responsive)
- `className?: string` - Additional CSS classes

### GradientButton
Beautiful gradient buttons with multiple variants and sizes.

```tsx
<GradientButton 
  variant="primary" // "primary" | "secondary" | "success" | "danger"
  size="md" // "sm" | "md" | "lg"
  fullWidth={false}
  onClick={handleClick}
>
  Click Me
</GradientButton>
```

**Props:**
- `variant?: "primary" | "secondary" | "success" | "danger"` - Button style
- `size?: "sm" | "md" | "lg"` - Button size
- `fullWidth?: boolean` - Whether button should take full width
- Extends standard button HTML attributes

### InfoSection
Informational section at the bottom of pages with gradient backgrounds.

```tsx
<InfoSection 
  icon={Settings} 
  title="About This Feature" 
  variant="blue" // "default" | "blue" | "green" | "purple"
>
  This is some helpful information about the feature.
</InfoSection>
```

**Props:**
- `icon: LucideIcon` - Icon to display
- `title: string` - Section title
- `variant?: "default" | "blue" | "green" | "purple"` - Color scheme
- `className?: string` - Additional CSS classes

## 🎨 Design System

### Color Palette
- **Background**: Dark gradient (gray-900 to gray-800)
- **Cards**: Semi-transparent white (white/5) with backdrop blur
- **Borders**: Subtle white borders (white/10)
- **Text**: White and gray variants for hierarchy
- **Accents**: Blue, purple, green, and pink gradients

### Spacing
- **Small**: 4 (1rem)
- **Medium**: 6 (1.5rem)
- **Large**: 8 (2rem)
- **Extra Large**: 12 (3rem)

### Typography
- **Headers**: Large, bold white text
- **Body**: Medium gray text for readability
- **Labels**: Small, medium-weight gray text

## 📱 Responsive Design

All components are fully responsive and work on:
- Mobile devices (1 column layouts)
- Tablets (2 column layouts)
- Desktop (3-4 column layouts)

## 🔧 Customization

Each component accepts `className` props for additional styling:

```tsx
<GlassCard className="my-custom-class">
  {/* Content */}
</GlassCard>
```

## 📚 Example Usage

See `ComponentShowcase.tsx` for a complete example of how to use all components together.

## 🎯 Best Practices

1. **Consistent Layout**: Use `PageLayout` for all pages
2. **Icon Consistency**: Use Lucide React icons throughout
3. **Color Variants**: Use appropriate color variants for different contexts
4. **Responsive Grids**: Use `StatusGrid` for organized information display
5. **Button Hierarchy**: Use appropriate button variants for different actions

## 🚀 Getting Started

1. Import the components you need
2. Wrap your page with `PageLayout`
3. Add a `PageHeader` for the main title
4. Use `GlassCard` for content sections
5. Organize information with `StatusCard` and `StatusGrid`
6. Add `InfoSection` for additional context
7. Use `GradientButton` for actions

This component library provides everything you need to create beautiful, consistent pages throughout your application! ✨
