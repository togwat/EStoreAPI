import { SkillSummary } from "@/api/skills";
import { Card } from "@/components/ui/card";

interface SkillCardProps {
    skillSummary: SkillSummary;
    onClick: () => void;
}

export default function SkillCard({ skillSummary, onClick }: SkillCardProps) {
    return (
        <Card
            className="border bg-muted transition-opacity hover:opacity-75"
            onClick={onClick}
            role={"button"}
        >
            <p>{skillSummary.name}</p>
        </Card>
    )
}