import { useIsMobile } from "@/hooks/use-mobile";
import { getSkill, listSkills, Skill, SkillSummary } from "@/api/skills";
import SkillCard from "./components/SkillCard";
import { useEffect, useState } from "react";
import { Button } from "@/components/ui/button";
import { ArrowLeft, DownloadIcon, PencilIcon } from "lucide-react";
import { cn } from "@/lib/utils";

export default function SkillsPage({ title }: { title: string }) {
    const isMobile = useIsMobile();
    const [summaries, setSummaries] = useState<SkillSummary[]>([]);
    const [selectedName, setSelectedName] = useState<string | null>(null);
    const [selectedSkill, setSelectedSkill] = useState<Skill | null>(null);

    useEffect(() => {
        listSkills().then(setSummaries);
    }, []);

    // use selectedName to switch to viewing/editing mode immediately,
    // and wait for content fetching there
    useEffect(() => {
        if (!selectedName) return;

        setSelectedSkill(null);

        getSkill(selectedName)
            .then((skill) => { setSelectedSkill(skill); })
            .catch(() => { setSelectedName(null); });
    }, [selectedName]);

    const toCard = (skillSummary: SkillSummary) => (
        <SkillCard
            key={skillSummary.name}
            skillSummary={skillSummary}
            onClick={() => setSelectedName(skillSummary.name)}
        />
    );

    return (
        <div>
            {selectedName ? (
                <div className={isMobile ? "" : "p-8"}>
                    {/** skill viewing/editing page */}
                    {/** header */}
                    <div className={cn("flex items-center justify-between", isMobile ? "pb-4" : "py-4")}>
                        <div className="flex items-center gap-2">
                            <Button variant="outline" size="icon" onClick={() => setSelectedName(null)}><ArrowLeft /></Button>
                            <h2 className="text-xl font-semibold">{selectedName}</h2>
                        </div>

                        <div className="flex items-center gap-2">
                            <Button size={isMobile ? "icon" : "lg"} variant="outline" onClick={() => {}}><DownloadIcon />{!isMobile && "Download"}</Button>
                            <Button size={isMobile ? "icon" : "lg"} onClick={() => {}}><PencilIcon />{!isMobile && "Edit"}</Button>
                        </div>
                    </div>
                    {/** markdown renderer/text editor */}
                    {selectedSkill && selectedSkill.file}
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