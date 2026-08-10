import { useIsMobile } from "@/hooks/use-mobile";
import { getSkill, listSkills, Skill, SkillSummary } from "@/api/skills";
import SkillCard from "./components/SkillCard";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { ArrowLeft, PencilIcon } from "lucide-react";

export default function SkillsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [summaries, setSummaries] = useState<SkillSummary[]>([]);
    const [selectedSkill, setSelectedSkill] = useState<Skill | null>(null);

    useEffect(() => {
        listSkills().then(setSummaries);
    }, []);

    const toCard = (skillSummary: SkillSummary) => (
        <SkillCard
            key={skillSummary.name}
            skillSummary={skillSummary}
            onClick={async () => setSelectedSkill(await getSkill(skillSummary.name))}
        />
    );

    return (
        <div>
            {selectedSkill ? (
                <div className={isMobile ? "" : "p-8"}>
                    {/** skill viewing/editing page */}
                    {/** header */}
                    { isMobile ? <div className="flex items-center justify-between">
                        <Button variant="outline" size="icon" onClick={() => setSelectedSkill(null)}><ArrowLeft /></Button>
                        <Button size="icon" onClick={() => {}}><PencilIcon /></Button>
                    </div>
                    : <div className="flex items-center justify-between">
                        <div className="flex items-center gap-2">
                            <Button variant="outline" size="icon" onClick={() => setSelectedSkill(null)}><ArrowLeft /></Button>
                            <h2>{selectedSkill.name}</h2>
                        </div>
                        <Button size="lg" onClick={() => {}}><PencilIcon />Edit Skill</Button>
                    </div>}
                    {/** markdown renderer/text editor */}
                    {selectedSkill.file}
                </div>
            ) : (
                <div className={isMobile ? "" : "p-8"}>
                    {/** skill cards grid/list */}
                    {/** header */}
                    { !isMobile && <h1 className="pb-4">{title}</h1> }
                    <span className="pl-1 text-muted-foreground">{summaries.length} skills</span>
                    {/** desktop grid, mobile rows */}
                    <div className={isMobile
                        ? "py-4 flex flex-col gap-2"
                        : "py-4 grid grid-cols-[repeat(auto-fill,_16rem)] gap-4"}>
                        {summaries.map(toCard)}
                    </div>
                </div>
            )}
        </div>
    )
}