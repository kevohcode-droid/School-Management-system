const fs = require('fs');
const path = require('path');

const dir = 'c:/PROJECTS/School-management-system-/src/SchoolErp.Frontend/src/app';

const emojiMap = {
  '←': 'arrow_back',
  '🔍': 'search',
  '📥': 'inbox',
  '📄': 'description',
  '✓': 'check',
  '✗': 'close',
  '🚪': 'logout',
  '✕': 'close',
  '🔐': 'lock',
  '➕': 'add',
  '✏': 'edit',
  '🗑': 'delete',
  '💾': 'save',
  '📋': 'assignment',
  '💰': 'payments',
  '🔑': 'key',
  '👤': 'person',
  '📝': 'note',
  '🏫': 'school',
  '✅': 'check_circle',
  '📚': 'book',
  '📍': 'location_on',
  '📖': 'menu_book',
  '🔄': 'sync',
  '📤': 'publish',
  '📊': 'bar_chart',
  '👨': 'man',
  '🎓': 'school',
  '👩': 'woman',
  '💳': 'credit_card',
  '📜': 'receipt_long',
  '⚠': 'warning',
  '📉': 'trending_down',
  '🧾': 'receipt',
  '🏷': 'label',
  '⚙': 'settings',
  '📱': 'smartphone',
  '🏦': 'account_balance',
  '💵': 'attach_money',
  '🖨': 'print',
  '📁': 'folder',
  '🛡': 'shield',
  '👥': 'group',
  '🔔': 'notifications',
  '❌': 'cancel',
  '📢': 'campaign',
  '📈': 'trending_up',
  '🏆': 'emoji_events',
  '🎯': 'ads_click',
  '🗓': 'calendar_today',
  '✉': 'mail',
  '💼': 'work',
  '📅': 'calendar_month',
  '→': 'arrow_forward',
  '🗄': 'inventory',
  '💸': 'money_off',
  '🟢': 'circle',
  '⚡': 'bolt',
  '🖥': 'desktop_windows'
};

function walk(directory) {
  const files = fs.readdirSync(directory);
  for (const file of files) {
    const fullPath = path.join(directory, file);
    if (fs.statSync(fullPath).isDirectory()) {
      walk(fullPath);
    } else if (fullPath.endsWith('.html') || fullPath.endsWith('.ts')) {
      let content = fs.readFileSync(fullPath, 'utf8');
      let changed = false;
      
      for (const [emoji, icon] of Object.entries(emojiMap)) {
        if (content.includes(emoji)) {
          const replacePattern = new RegExp(emoji, 'g');
          content = content.replace(replacePattern, `<span class="material-symbols-outlined" style="vertical-align: middle; font-size: inherit;">${icon}</span>`);
          changed = true;
        }
      }

      if (changed) {
        fs.writeFileSync(fullPath, content);
        console.log(`Updated ${fullPath}`);
      }
    }
  }
}
walk(dir);
