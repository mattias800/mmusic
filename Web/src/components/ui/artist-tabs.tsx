import * as React from "react";
import { cn } from "@/lib/utils";

export interface ArtistTabsProps {
  tabs: {
    id: string;
    label: string;
    icon?: React.ComponentType<{ className?: string }>;
    content: React.ReactNode;
  }[];
  defaultTab?: string;
  className?: string;
}

export const ArtistTabs: React.FC<ArtistTabsProps> = ({ tabs, defaultTab, className }) => {
  const [activeTab, setActiveTab] = React.useState(defaultTab || tabs[0]?.id);

  if (!tabs.length) return null;

  return (
    <div className={cn("space-y-6", className)}>
      {/* Tab Navigation */}
      <div className="flex space-x-1 bg-white/5 backdrop-blur-sm rounded-xl p-1 border border-white/10">
        {tabs.map((tab) => {
          const Icon = tab.icon;
          const isActive = activeTab === tab.id;
          
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id)}
              className={cn(
                "flex items-center gap-2 px-4 py-2.5 rounded-lg text-sm font-medium transition-all duration-200",
                "hover:bg-white/10 hover:text-white",
                isActive
                  ? "bg-gradient-to-r from-blue-500/20 to-purple-500/20 text-white border border-blue-500/30 shadow-lg"
                  : "text-gray-300 hover:text-white"
              )}
            >
              {Icon && <Icon className="w-4 h-4" />}
              {tab.label}
            </button>
          );
        })}
      </div>

      {/* Tab Content */}
      <div className="min-h-[200px]">
        {tabs.find(tab => tab.id === activeTab)?.content}
      </div>
    </div>
  );
};
