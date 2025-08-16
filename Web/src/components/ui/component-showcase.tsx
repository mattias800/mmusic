import React from "react";
import {
  PageLayout,
  PageHeader,
  GlassCard,
  StatusCard,
  StatusGrid,
  GradientButton,
  InfoSection,
} from "@/components/ui";
import {
  User,
  Settings,
  Music,
  Calendar,
  Star,
  Heart,
  Play,
  Download,
  Search,
} from "lucide-react";

export const ComponentShowcase: React.FC = () => {
  return (
    <PageLayout>
      {/* Header Section */}
      <PageHeader
        icon={Star}
        title="UI Component Library"
        subtitle="Beautiful, reusable components for your pages"
      />

      {/* Main Content Grid */}
      <div className="grid grid-cols-1 xl:grid-cols-2 gap-8 max-w-7xl mx-auto">
        {/* Left Column - Component Examples */}
        <div className="space-y-8">
          {/* GlassCard Examples */}
          <GlassCard
            title="Glass Card Component"
            icon={Settings}
            iconBgColor="bg-blue-500/20"
          >
            <p className="text-gray-300 mb-4">
              This is a beautiful glass card with backdrop blur effects and
              subtle borders.
            </p>
            <div className="space-y-3">
              <StatusCard label="Feature" value="Glass Morphism" />
              <StatusCard label="Style" value="Modern & Clean" />
              <StatusCard label="Theme" value="Dark Mode" />
            </div>
          </GlassCard>

          {/* Status Grid Examples */}
          <GlassCard
            title="Status Grid Layout"
            icon={Music}
            iconBgColor="bg-green-500/20"
          >
            <StatusGrid columns={2}>
              <StatusCard label="Username" value="musiclover" icon={User} />
              <StatusCard label="Joined" value="2024" icon={Calendar} />
              <StatusCard label="Playlists" value="12" icon={Play} />
              <StatusCard label="Downloads" value="45" icon={Download} />
            </StatusGrid>
          </GlassCard>
        </div>

        {/* Right Column - Button Examples */}
        <div className="space-y-8">
          {/* Button Variants */}
          <GlassCard
            title="Gradient Button Variants"
            icon={Heart}
            iconBgColor="bg-pink-500/20"
          >
            <div className="space-y-4">
              <GradientButton variant="primary" fullWidth>
                Primary Action
              </GradientButton>
              <GradientButton variant="success" fullWidth>
                Success Action
              </GradientButton>
              <GradientButton variant="danger" fullWidth>
                Danger Action
              </GradientButton>
              <GradientButton variant="secondary" fullWidth>
                Secondary Action
              </GradientButton>
            </div>
          </GlassCard>

          {/* Button Sizes */}
          <GlassCard
            title="Button Sizes"
            icon={Search}
            iconBgColor="bg-purple-500/20"
          >
            <div className="space-y-3">
              <GradientButton variant="primary" size="sm">
                Small Button
              </GradientButton>
              <GradientButton variant="primary" size="md">
                Medium Button
              </GradientButton>
              <GradientButton variant="primary" size="lg">
                Large Button
              </GradientButton>
            </div>
          </GlassCard>
        </div>
      </div>

      {/* Info Sections */}
      <div className="mt-12 space-y-8 max-w-4xl mx-auto">
        <InfoSection
          icon={Settings}
          title="How to Use These Components"
          variant="blue"
        >
          Simply import the components you need from{" "}
          <code className="bg-white/10 px-2 py-1 rounded">@/components/ui</code>{" "}
          and start building beautiful pages. Each component is fully
          customizable with props for colors, sizes, and layouts.
        </InfoSection>

        <InfoSection icon={Star} title="Design System" variant="purple">
          This design system provides consistent spacing, colors, and typography
          across your application. All components use the same dark theme with
          beautiful gradients and glass morphism effects.
        </InfoSection>
      </div>
    </PageLayout>
  );
};
