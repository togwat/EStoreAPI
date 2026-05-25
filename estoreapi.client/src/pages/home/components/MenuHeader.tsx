import { getTheme } from '../../../lib/theme';
import { themeLogos } from '../../../lib/themeLogos';

export default function MenuHeader() {
    return (
        <div className="flex items-center justify-center gap-4 flex-wrap mb-16">
            <img src={themeLogos[getTheme()]} className="h-10" />
            <h1 className="m-0">Job Management Console</h1>
        </div>
    );
}