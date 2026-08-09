import { SkillSummary } from "@/api/skills";
import { Card, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FileText } from "lucide-react";

interface SkillCardProps {
    skillSummary: SkillSummary;
    onClick: () => void;
}

export default function SkillCard({ skillSummary, onClick }: SkillCardProps) {
    return (
        <Card
            className="border border-border cursor-pointer hover:border-foreground/50 transition-all w-full aspect-square"
            onClick={onClick}
            role={"button"}
        >
            <div className="mx-4 flex size-10 items-center justify-center rounded-lg bg-accent">
                <FileText className="size-5 text-accent-foreground" />
            </div>
            <CardHeader className="gap-2 overflow-hidden">
                <CardTitle className="font-semibold wrap-anywhere">{skillSummary.name}</CardTitle>
                <CardDescription className="wrap-anywhere">{skillSummary.summary}</CardDescription>
            </CardHeader>
        </Card>
    )
}