import { Card } from "@/components/ui/card";
import { useIsMobile } from "@/hooks/use-mobile";
import { cn } from "@/lib/utils";
import { Plus } from "lucide-react";

interface NewSkillCardProps {
    onClick: () => void;
}

export default function NewSkillCard({ onClick }: NewSkillCardProps) {
    const isMobile = useIsMobile();

    return (
        <Card
            size={isMobile ? "sm" : "default"}
            className={cn(
                "border border-dashed border-foreground/50 cursor-pointer w-full text-muted-foreground bg-background",
                !isMobile && "aspect-square"
            )}
            onClick={onClick}
            role={"button"}
        >
            {/** min-h-11 approximation of mobile card height */}
            <div className={cn("flex flex-1 items-center justify-center", isMobile && "min-h-11")}>
                <Plus className="size-6" />
            </div>
        </Card>
    )
}
