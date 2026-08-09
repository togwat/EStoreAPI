import { SkillSummary } from "@/api/skills";
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { useIsMobile } from "@/hooks/use-mobile";
import { cn } from "@/lib/utils";
import { FileText } from "lucide-react";

interface SkillCardProps {
    skillSummary: SkillSummary;
    onClick: () => void;
}

export default function SkillCard({ skillSummary, onClick }: SkillCardProps) {
    const isMobile = useIsMobile();

    return (
        <Card
            size={isMobile ? "sm" : "default"}
            className={cn(
                "border border-border cursor-pointer hover:border-foreground/50 transition-all w-full",
                isMobile ? "flex-row items-center gap-0!" : "aspect-square"
            )}
            onClick={onClick}
            role={"button"}
        >
            <div className={cn(
                "flex size-10 items-center justify-center rounded-lg bg-accent",
                isMobile ? "ml-3" : "mx-4"
            )}>
                <FileText className="size-5 text-accent-foreground" />
            </div>

            <CardHeader className={cn("gap-2 overflow-hidden", isMobile && "flex-1")}>
                <CardTitle className="font-semibold wrap-anywhere">{skillSummary.name}</CardTitle>
                <CardDescription className="wrap-anywhere">{skillSummary.summary}</CardDescription>
            </CardHeader>
        </Card>
    )
}