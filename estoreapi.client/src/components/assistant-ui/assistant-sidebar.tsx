import {
  ResizableHandle,
  ResizablePanel,
  ResizablePanelGroup,
} from "src/components/ui/resizable";
import type { FC, PropsWithChildren } from "react";

import { Thread } from "src/components/assistant-ui/thread";

import { useIsMobile } from "@/hooks/use-mobile";

export const AssistantSidebar: FC<PropsWithChildren> = ({ children }) => {
  const isMobile = useIsMobile();
  return (
    <ResizablePanelGroup orientation={isMobile ? "vertical" : "horizontal"}>
      <ResizablePanel>{children}</ResizablePanel>
      <ResizableHandle />
      <ResizablePanel>
        <Thread />
      </ResizablePanel>
    </ResizablePanelGroup>
  );
};
