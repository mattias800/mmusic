import * as React from "react";
import { useState } from "react";
import { GlassCard, InfoSection, PageHeader, PageLayout } from "@/components/ui";
import { Button } from "@/components/ui/button.tsx";
import { MmusicLogo } from "@/components/logo/MmusicLogo.tsx";
import { InitialSetupPanel } from "@/features/first-time-setup/InitialSetupPanel.tsx";
import { AuthTokensStatusPanel } from "@/features/settings/AuthTokensStatusPanel.tsx";
import { ExternalLink, Settings, ShieldCheck, Info } from "lucide-react";

export interface InstallationWizardProps {}

export const InstallationWizard: React.FC<InstallationWizardProps> = () => {
  const [stepIndex, setStepIndex] = useState<number>(0);

  const steps: { title: string; content: React.ReactNode }[] = [
    {
      title: "Welcome",
      content: (
        <GlassCard title="Welcome to mmusic" icon={ExternalLink} iconBgColor="bg-blue-500/20">
          <div className="flex flex-col items-center gap-6">
            <MmusicLogo width={"220px"} />
            <div className="text-sm opacity-80 text-center max-w-prose">
              Thanks for installing mmusic. Start by reviewing the project documentation and repository.
            </div>
            <div className="flex gap-3">
              <a className="underline text-blue-400" href="https://github.com" target="_blank" rel="noreferrer">
                Project on GitHub
              </a>
              <a className="underline text-blue-400" href="https://github.com" target="_blank" rel="noreferrer">
                Documentation
              </a>
            </div>
          </div>
        </GlassCard>
      ),
    },
    {
      title: "External services status",
      content: (
        <GlassCard title="External Services Tokens" icon={ShieldCheck} iconBgColor="bg-green-500/20">
          <div className="space-y-4">
            <div className="text-sm opacity-80">
              Below is the status of configured tokens for external services. Configure these via environment variables.
            </div>
            <AuthTokensStatusPanel />
          </div>
        </GlassCard>
      ),
    },
    {
      title: "Helpful information",
      content: (
        <GlassCard title="Next Steps" icon={Info} iconBgColor="bg-yellow-500/20">
          <div className="space-y-3 text-sm opacity-90">
            <div>
              You can test connectivity to external services from the Server Settings status page.
            </div>
            <div>
              Integration with download services (Prowlarr, qBittorrent, etc.) must be configured in the Server Settings page.
            </div>
            <div>
              Go to <a className="underline text-blue-400" href="/settings">Settings</a> any time to review and test configurations.
            </div>
          </div>
        </GlassCard>
      ),
    },
    {
      title: "Create admin user",
      content: (
        <GlassCard title="Create Admin Account" icon={Settings} iconBgColor="bg-purple-500/20">
          <div className="text-sm opacity-80 mb-4">
            Create the first admin account to complete setup.
          </div>
          <InitialSetupPanel />
        </GlassCard>
      ),
    },
  ];

  const isFirst = stepIndex === 0;
  const isLast = stepIndex === steps.length - 1;

  return (
    <PageLayout>
      <PageHeader
        icon={Settings}
        title="Installation Wizard"
        subtitle={`Step ${stepIndex + 1} of ${steps.length}: ${steps[stepIndex].title}`}
      />

      <div className="max-w-3xl mx-auto space-y-6">
        {steps[stepIndex].content}

        <div className="flex justify-between">
          <Button variant="secondary" disabled={isFirst} onClick={() => setStepIndex((i) => Math.max(0, i - 1))}>
            Back
          </Button>
          {!isLast && (
            <Button onClick={() => setStepIndex((i) => Math.min(steps.length - 1, i + 1))}>Next</Button>
          )}
        </div>

        <InfoSection icon={Info} title="Tips" variant="blue">
          You can revisit this information later from the Settings page. The admin account creation is required to continue.
        </InfoSection>
      </div>
    </PageLayout>
  );
};


