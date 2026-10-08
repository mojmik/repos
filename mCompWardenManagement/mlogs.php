<?php
/**
 * mlogs.php - single-file internal monitoring system
 * - Dashboard / Log Explorer / PC Detail
 * - AJAX API served from same file (?api=...)
 * - Keeps existing putlog endpoint: ?action=putlog&comp=...&user=...&opt=...&val=...&dt=...
 */

ini_set("memory_limit","1024M");
ini_set('display_errors', 1);
ini_set('error_reporting', E_ALL & ~E_NOTICE);

// Keep your existing include if you need getReq(), session, auth, etc.
require_once "../wp-content/themes/intra2/m_intra.php";
if (session_status() !== PHP_SESSION_ACTIVE) session_start();

/* -----------------------------
   Config
------------------------------ */
define('DB_NAME', 'wordpress315');
define('DB_USER', 'wordpressuser315');
define('DB_PASSWORD', '--Wo84&#0g|C');
define('DB_HOST', 'localhost');

define('DEFAULT_LIMIT', 100);
define('DASHBOARD_LIMIT', 300); // how many PCs max shown
define('PC_LOG_LIMIT', 500);
define('OFFLINE_MINUTES', 15); // highlight if last_seen older than this
define('MCW_LATEST_VERSION', 'v5.02');
define('MCW_COMMAND_SHARE_PATH', '\\\\rentex.intra\\company\\data\\Company\\mkavan_upravy\\scripts\\mCompWarden2\\');
define('MCW_UPDATER_EXE', '\\\\rentex.intra\\SYSVOL\\rentex.intra\\scripts\\mcompwarden2\\MCompWardenUpdater.exe');

/* -----------------------------
   Helpers (compatible with your old style)
------------------------------ */
function req($k, $default = '') {
  return isset($_GET[$k]) ? $_GET[$k] : (isset($_POST[$k]) ? $_POST[$k] : $default);
}
function h($s) { return htmlspecialchars((string)$s, ENT_QUOTES, 'UTF-8'); }
function nowStr() { return date('Y-m-d H:i:s'); }
function toDateTimeOrEmpty($d) {
  // Accepts YYYY-MM-DD or YYYY-MM-DD HH:MM:SS or your legacy "j. m. Y, H:i:s"
  if (!$d) return '';
  $d = trim($d);

  if (preg_match('/^\d{4}-\d{2}-\d{2}$/', $d)) return $d . ' 00:00:00';
  if (preg_match('/^\d{4}-\d{2}-\d{2}\s+\d{2}:\d{2}:\d{2}$/', $d)) return $d;

  // legacy: "j. m. Y, H:i:s"
  $d2 = str_replace(['. ', ','], ['-', ''], $d);
  $ts = strtotime($d2);
  if ($ts) return date('Y-m-d H:i:s', $ts);

  return '';
}
function mcwNormalizeVersion($v) {
  $v = trim((string)$v);
  if ($v === '') return '';
  return ltrim($v, "vV");
}
function mcwIsOutdatedVersion($current, $latest = MCW_LATEST_VERSION) {
  $current = mcwNormalizeVersion($current);
  $latest = mcwNormalizeVersion($latest);
  if ($current === '' || $latest === '') return false;
  return version_compare($current, $latest, '<');
}

/* -----------------------------
   DB
------------------------------ */
final class DB {
  private static $pdo;

  public static function init() {
    if (self::$pdo) return;
    self::$pdo = new PDO(
      "mysql:host=".DB_HOST.";dbname=".DB_NAME.";charset=utf8",
      DB_USER,
      DB_PASSWORD,
      [
        PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
        PDO::ATTR_EMULATE_PREPARES => false,
      ]
    );
    self::ensureSchema();
  }
  private static function ensureSchema() {
    self::$pdo->exec("
      CREATE TABLE IF NOT EXISTS m_status (
        id INT UNSIGNED NOT NULL AUTO_INCREMENT,
        comp VARCHAR(120) NOT NULL,
        logged_users VARCHAR(255) NOT NULL DEFAULT '',
        client_version VARCHAR(50) NOT NULL DEFAULT '',
        ip_addresses TEXT NULL,
        ping_ms INT NULL,
        agent_context VARCHAR(50) NOT NULL DEFAULT '',
        os_version VARCHAR(255) NOT NULL DEFAULT '',
        boot_time DATETIME NULL,
        last_seen DATETIME NOT NULL,
        updated_at DATETIME NOT NULL,
        PRIMARY KEY (id),
        UNIQUE KEY uq_comp (comp),
        KEY idx_last_seen (last_seen)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

      CREATE TABLE IF NOT EXISTS m_logs (
        id INT UNSIGNED NOT NULL AUTO_INCREMENT,
        comp VARCHAR(120) NOT NULL,
        user VARCHAR(120) NOT NULL DEFAULT '',
        opt VARCHAR(120) NOT NULL DEFAULT '',
        val TEXT NULL,
        gtime DATETIME NOT NULL,
        PRIMARY KEY (id),
        KEY idx_comp_opt (comp, opt),
        KEY idx_gtime (gtime),
        KEY idx_comp_gtime (comp, gtime)
      ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
    ");
  }
  public static function q($sql, $params = []) {
    self::init();
    $st = self::$pdo->prepare($sql);
    $st->execute($params);
    return $st;
  }
  public static function all($sql, $params = []) {
    return self::q($sql, $params)->fetchAll(PDO::FETCH_ASSOC);
  }
  public static function one($sql, $params = []) {
    return self::q($sql, $params)->fetch(PDO::FETCH_ASSOC);
  }
  public static function val($sql, $params = []) {
    $row = self::one($sql, $params);
    if (!$row) return null;
    return array_values($row)[0];
  }
  public static function lastId() {
    return self::$pdo ? self::$pdo->lastInsertId() : null;
  }
}

/* -----------------------------
   Model
------------------------------ */
final class LogModel {

  public function putLog($comp, $user, $opt, $val, $dt) {
    if (!$comp) return null;
    $dt = toDateTimeOrEmpty($dt);
    if (!$dt) $dt = nowStr();

    if ($opt === 'ip') {
      DB::q("DELETE FROM m_logs WHERE opt='ip' AND comp=?", [$comp]);
    }

    DB::q(
      "INSERT INTO m_logs (comp,user,opt,val,gtime) VALUES (?,?,?,?,?)",
      [$comp, $user, $opt, $val, $dt]
    );

    $id = DB::lastId();

    // Auto cleanup (1% chance to keep performance high)
    if (rand(1, 100) === 1) {
      $this->removeOld(7);
    }

    return $id;
  }

  public function putHeartbeat($payload) {
    $comp = trim((string)($payload['comp'] ?? ''));
    if ($comp === '') return null;

    $dt = toDateTimeOrEmpty($payload['dt'] ?? '');
    if (!$dt) $dt = nowStr();

    $bootTime = toDateTimeOrEmpty($payload['boot_time'] ?? '');
    if (!$bootTime) $bootTime = null;

    $pingMs = trim((string)($payload['ping_ms'] ?? ''));
    $pingMs = ($pingMs === '' || !is_numeric($pingMs)) ? null : intval($pingMs);

    DB::q(
      "INSERT INTO m_status
        (comp, logged_users, client_version, ip_addresses, ping_ms, agent_context, os_version, boot_time, last_seen, updated_at)
       VALUES
        (?, ?, ?, ?, ?, ?, ?, ?, ?, ?)
       ON DUPLICATE KEY UPDATE
        logged_users = VALUES(logged_users),
        client_version = VALUES(client_version),
        ip_addresses = VALUES(ip_addresses),
        ping_ms = VALUES(ping_ms),
        agent_context = VALUES(agent_context),
        os_version = VALUES(os_version),
        boot_time = VALUES(boot_time),
        last_seen = VALUES(last_seen),
        updated_at = VALUES(updated_at)",
      [
        $comp,
        (string)($payload['users'] ?? ''),
        (string)($payload['client_version'] ?? ''),
        (string)($payload['ip_addresses'] ?? ''),
        $pingMs,
        (string)($payload['agent_context'] ?? ''),
        (string)($payload['os_version'] ?? ''),
        $bootTime,
        $dt,
        nowStr()
      ]
    );

    return DB::lastId();
  }

  public function queueRunProgramCommand($comp, $programPath, $args = '') {
    $comp = trim((string)$comp);
    $programPath = trim((string)$programPath);
    if ($comp === '') throw new RuntimeException('Missing computer name.');
    if (!preg_match('/^[A-Za-z0-9._-]+$/', $comp)) throw new RuntimeException('Invalid computer name.');
    if ($programPath === '') throw new RuntimeException('Missing program path.');

    $dir = MCW_COMMAND_SHARE_PATH;
    if (!is_dir($dir)) throw new RuntimeException('Command share path is not available.');

    $taskId = 'dashboard-update-' . date('Ymd-His') . '-' . substr(md5($comp . microtime(true)), 0, 8);
    $fileName = strtolower($comp) . '-update-' . date('Ymd-His') . '.mcw3.xml';
    $fullPath = rtrim($dir, "\\/") . DIRECTORY_SEPARATOR . $fileName;

    $xml =
'<?xml version="1.0" encoding="utf-8"?>' . "\n" .
'<Tasks defaultTimezone="Europe/Prague">' . "\n" .
'  <Task id="' . h($taskId) . '" enabled="true" machine="' . h($comp) . '" runAs="system" needsNetwork="true">' . "\n" .
'    <Description>Queued from mLogs dashboard</Description>' . "\n" .
'    <Schedule type="once" />' . "\n" .
'    <Actions>' . "\n" .
'      <Action type="RunProgram" file="' . h($programPath) . '"' . ($args !== '' ? ' args="' . h($args) . '"' : '') . ' />' . "\n" .
'    </Actions>' . "\n" .
'  </Task>' . "\n" .
'</Tasks>' . "\n";

    $written = @file_put_contents($fullPath, $xml, LOCK_EX);
    if ($written === false) throw new RuntimeException('Failed to write command file.');

    return $fileName;
  }

  public function queueCustomRunCommand($comp, $programPath, $args = '') {
    $programPath = trim((string)$programPath);
    $args = trim((string)$args);
    if ($programPath === '') throw new RuntimeException('Program path is required.');
    return $this->queueRunProgramCommand($comp, $programPath, $args);
  }

  public function queueSysInfoCommand($comp) {
    $comp = trim((string)$comp);
    if ($comp === '') throw new RuntimeException('Missing computer name.');
    if (!preg_match('/^[A-Za-z0-9._-]+$/', $comp)) throw new RuntimeException('Invalid computer name.');

    $dir = MCW_COMMAND_SHARE_PATH;
    if (!is_dir($dir)) throw new RuntimeException('Command share path is not available.');

    $taskId = 'dashboard-sysinfo-' . date('Ymd-His') . '-' . substr(md5($comp . microtime(true)), 0, 8);
    $fileName = strtolower($comp) . '-sysinfo-' . date('Ymd-His') . '.mcw3.xml';
    $fullPath = rtrim($dir, "\\/") . DIRECTORY_SEPARATOR . $fileName;

    $xml =
'<?xml version="1.0" encoding="utf-8"?>' . "\n" .
'<Tasks defaultTimezone="Europe/Prague">' . "\n" .
'  <Task id="' . h($taskId) . '" enabled="true" machine="' . h($comp) . '" runAs="user" needsNetwork="true">' . "\n" .
'    <Description>System info request from mLogs</Description>' . "\n" .
'    <Schedule type="once" />' . "\n" .
'    <Actions>' . "\n" .
'      <Action type="PostMessage" widget="summary" message="#summary" />' . "\n" .
'      <Action type="PostMessage" widget="ram" message="#ram" />' . "\n" .
'      <Action type="PostMessage" widget="free_c" message="#free_c" />' . "\n" .
'      <Action type="PostMessage" widget="printers" message="#printers" />' . "\n" .
'    </Actions>' . "\n" .
'  </Task>' . "\n" .
'</Tasks>' . "\n";

    $written = @file_put_contents($fullPath, $xml, LOCK_EX);
    if ($written === false) throw new RuntimeException('Failed to write command file.');

    return $fileName;
  }

  public function removeOld($days = 7) {
    $dt2 = date("Y-m-d H:i:s", strtotime("-{$days} days"));

    // 1) Delete everything older than N days
    DB::q("DELETE FROM m_logs WHERE gtime < ?", [$dt2]);

    // 2) Deduplicate: keep only the latest row per (comp, user, opt)
    DB::q("CREATE TEMPORARY TABLE IF NOT EXISTS temp_latest_logs
           SELECT MAX(id) AS latest_id
           FROM m_logs
           GROUP BY comp, user, opt");

    DB::q("DELETE l FROM m_logs l
           LEFT JOIN temp_latest_logs t ON l.id = t.latest_id
           WHERE t.latest_id IS NULL");

    DB::q("DROP TEMPORARY TABLE IF EXISTS temp_latest_logs");
  }

  public function loadFiles($dir = 'data') {
    if (!is_dir($dir)) {
        @mkdir($dir, 0777, true);
        return 0;
    }
    $arc = $dir . "/arc";
    if (!is_dir($arc)) @mkdir($arc, 0777, true);

    $files = glob($dir . "/*.txt");
    $recWritten = 0;
    $now = date('Y-m-d H:i:s');

    foreach ($files as $file) {
      $handle = fopen($file, "r");
      if (!$handle) continue;

      while (($line = fgets($handle)) !== false) {
        $line = trim(str_replace('"', '', $line));
        if (!$line) continue;
        $mExp = explode(';', $line, 4);
        if (count($mExp) < 3) continue;

        $m0 = $mExp[0]; // comp
        $m1 = $mExp[1]; // user
        $m2 = $mExp[2]; // opt
        $m3 = isset($mExp[3]) ? $mExp[3] : ''; // val
        
        $m3 = str_replace('\\', '\\\\', $m3);

        $this->putLog($m0, $m1, $m2, $m3, $now);
        $recWritten++;
      }
      fclose($handle);
      @rename($file, $arc . "/" . basename($file));
    }

    // Cleanup archive (older than 7 days)
    $arcFiles = glob($arc . "/*.txt");
    $limitSec = 7 * 24 * 60 * 60;
    foreach ($arcFiles as $f) {
      if (time() - filemtime($f) > $limitSec) @unlink($f);
    }
    return $recWritten;
  }

  public function distinct($col, $limit = 5000) {
    $allowed = ['comp','user','opt'];
    if (!in_array($col, $allowed, true)) return [];
    return DB::all("SELECT DISTINCT `$col` AS v FROM m_logs ORDER BY `$col` ASC LIMIT {$limit}");
  }

  public function searchLogs($filters, $sort, $dir, $limit, $offset) {
    $where = [];
    $p = [];

    if (!empty($filters['comp'])) { $where[]="comp = ?"; $p[]=$filters['comp']; }
    if (!empty($filters['user'])) { $where[]="user = ?"; $p[]=$filters['user']; }
    if (!empty($filters['opt']))  { $where[]="opt = ?";  $p[]=$filters['opt'];  }

    if (!empty($filters['q'])) {
      $where[]="(comp LIKE ? OR user LIKE ? OR opt LIKE ? OR val LIKE ?)";
      $like = "%".$filters['q']."%";
      array_push($p, $like, $like, $like, $like);
    }

    if (!empty($filters['from'])) { $where[]="gtime >= ?"; $p[]=$filters['from']; }
    if (!empty($filters['to']))   { $where[]="gtime <= ?"; $p[]=$filters['to'];   }

    $whereSql = $where ? "WHERE ".implode(" AND ", $where) : "";

    $allowedSort = ['comp','user','opt','val','gtime','id'];
    if (!in_array($sort, $allowedSort, true)) $sort = 'gtime';
    $dir = strtoupper($dir)==='ASC' ? 'ASC' : 'DESC';

    $rows = DB::all("
      SELECT id, comp, user, opt, val, gtime
      FROM m_logs
      $whereSql
      ORDER BY $sort $dir
      LIMIT ".intval($limit)." OFFSET ".intval($offset)."
    ", $p);

    $total = DB::val("
      SELECT COUNT(*) FROM m_logs $whereSql
    ", $p);

    return [$rows, intval($total)];
  }

  public function latestStatePerPC($limit = DASHBOARD_LIMIT) {
    return $this->latestStatePerPCFiltered($limit, '', '');
  }

  public function latestStatePerPCFiltered($limit = DASHBOARD_LIMIT, $userFilter = '', $compFilter = '') {
    $where = [];
    $params = [];

    if ($userFilter !== '') {
      $where[] = "logged_users LIKE ?";
      $params[] = "%".$userFilter."%";
    }
    if ($compFilter !== '') {
      $where[] = "comp LIKE ?";
      $params[] = "%".$compFilter."%";
    }

    $whereSql = $where ? ("WHERE ".implode(" AND ", $where)) : "";

    $statuses = DB::all("
      SELECT comp, last_seen, logged_users, client_version, ip_addresses, ping_ms, agent_context, os_version, boot_time
      FROM m_status
      $whereSql
      ORDER BY last_seen DESC
      LIMIT ".intval($limit)."
    ", $params);

    $isStatusBranch = !empty($statuses);
    if ($isStatusBranch) {
      $compNames = array_column($statuses, 'comp');
    } else {
      $pcs = DB::all("
        SELECT comp, MAX(gtime) AS last_seen
        FROM m_logs
        GROUP BY comp
        ORDER BY last_seen DESC
        LIMIT ".intval($limit)."
      ");
      if (!$pcs) return [];
      $compNames = array_column($pcs, 'comp');
    }

    $placeholders = implode(',', array_fill(0, count($compNames), '?'));

    // Batch fetch latest values for common options (RAM, Free C, Printers, Summary, etc.)
    $valRows = DB::all("
      SELECT l.comp, l.opt, l.val
      FROM m_logs l
      INNER JOIN (
          SELECT comp, opt, MAX(id) as max_id
          FROM m_logs
          WHERE comp IN ($placeholders) AND opt IN ('ip', 'ping', 'officeverze', 'sigmakra', 'ram', 'free_c', 'disk', 'disk_c', 'printers', 'summary', 'info')
          GROUP BY comp, opt
      ) latest ON l.id = latest.max_id
    ", $compNames);

    $valLookup = [];
    foreach ($valRows as $vr) {
      $valLookup[$vr['comp']][$vr['opt']] = $vr['val'];
    }

    // Batch fetch the absolute latest log entry (opt, val, gtime)
    $lastLogRows = DB::all("
      SELECT l.comp, l.opt, l.val, l.gtime
      FROM m_logs l
      INNER JOIN (
          SELECT comp, MAX(id) as max_id
          FROM m_logs
          WHERE comp IN ($placeholders)
          GROUP BY comp
      ) latest ON l.id = latest.max_id
    ", $compNames);

    $lastLogLookup = [];
    foreach ($lastLogRows as $llr) {
      $lastLogLookup[$llr['comp']] = $llr;
    }

    $out = [];
    if ($isStatusBranch) {
      foreach ($statuses as $r) {
        $comp = $r['comp'];
        $ll = $lastLogLookup[$comp] ?? ['opt' => '', 'val' => '', 'gtime' => ''];
        $vl = $valLookup[$comp] ?? [];
        $out[] = [
          'comp' => $comp,
          'last_seen' => $r['last_seen'],
          'users' => $r['logged_users'],
          'version' => $r['client_version'],
          'ip' => $r['ip_addresses'],
          'ping' => $r['ping_ms'],
          'context' => $r['agent_context'],
          'os_version' => $r['os_version'],
          'boot_time' => $r['boot_time'],
          'last_opt' => $ll['opt'],
          'last_val' => $ll['val'],
          'last_val_time' => $ll['gtime'],
          'ram' => $vl['ram'] ?? '',
          'free_c' => $vl['free_c'] ?? ($vl['disk_c'] ?? ($vl['disk'] ?? '')),
          'printers' => $vl['printers'] ?? '',
          'summary' => $vl['summary'] ?? ($vl['info'] ?? ''),
          'office' => $vl['officeverze'] ?? '',
          'sig' => $vl['sigmakra'] ?? '',
        ];
      }
    } else {
      foreach ($pcs as $r) {
        $comp = $r['comp'];
        $ll = $lastLogLookup[$comp] ?? ['opt' => '', 'val' => '', 'gtime' => ''];
        $vl = $valLookup[$comp] ?? [];
        $out[] = [
          'comp' => $comp,
          'last_seen' => $r['last_seen'],
          'users' => '',
          'version' => '',
          'ip' => $vl['ip'] ?? '',
          'ping' => $vl['ping'] ?? '',
          'context' => '',
          'os_version' => '',
          'boot_time' => '',
          'last_opt' => $ll['opt'],
          'last_val' => $ll['val'],
          'last_val_time' => $ll['gtime'],
          'ram' => $vl['ram'] ?? '',
          'free_c' => $vl['free_c'] ?? ($vl['disk_c'] ?? ($vl['disk'] ?? '')),
          'printers' => $vl['printers'] ?? '',
          'summary' => $vl['summary'] ?? ($vl['info'] ?? ''),
          'office' => $vl['officeverze'] ?? '',
          'sig' => $vl['sigmakra'] ?? '',
        ];
      }
    }
    return $out;
  }

  public function usersSummary($limit = DASHBOARD_LIMIT) {
    $statuses = DB::all("
      SELECT comp, last_seen, logged_users
      FROM m_status
      WHERE logged_users <> ''
      ORDER BY last_seen DESC
      LIMIT ".intval($limit)."
    ");

    $users = [];
    foreach ($statuses as $row) {
      $names = preg_split('/\s*-\s*/', (string)$row['logged_users']);
      foreach ($names as $name) {
        $name = trim($name);
        if ($name === '') continue;
        if (!isset($users[$name])) {
          $users[$name] = [
            'user' => $name,
            'pcs' => [],
            'last_seen' => '',
            'online' => 0,
            'late' => 0,
            'offline' => 0,
          ];
        }

        $users[$name]['pcs'][] = [
          'comp' => $row['comp'],
          'last_seen' => $row['last_seen'],
        ];

        if ($users[$name]['last_seen'] === '' || strtotime($row['last_seen']) > strtotime($users[$name]['last_seen'])) {
          $users[$name]['last_seen'] = $row['last_seen'];
        }

        $mins = $row['last_seen'] ? (time() - strtotime($row['last_seen'])) / 60 : 999999;
        if ($mins <= 2) $users[$name]['online']++;
        else if ($mins <= OFFLINE_MINUTES) $users[$name]['late']++;
        else $users[$name]['offline']++;
      }
    }

    uasort($users, function($a, $b) {
      $ac = count($a['pcs']);
      $bc = count($b['pcs']);
      if ($ac === $bc) {
        return strcmp($a['user'], $b['user']);
      }
      return $bc <=> $ac;
    });

    return array_values($users);
  }

  public function pcStatus($comp) {
    return DB::one("
      SELECT comp, last_seen, logged_users, client_version, ip_addresses, ping_ms, agent_context, os_version, boot_time
      FROM m_status
      WHERE comp=?
      LIMIT 1
    ", [$comp]);
  }

  public function latestVal($comp, $opt) {
    $row = DB::one("
      SELECT val, gtime
      FROM m_logs
      WHERE comp=? AND opt=?
      ORDER BY gtime DESC
      LIMIT 1
    ", [$comp, $opt]);
    return $row ? $row['val'] : '';
  }

  public function latestOpt($comp) {
    $row = DB::one("
      SELECT opt
      FROM m_logs
      WHERE comp=?
      ORDER BY gtime DESC
      LIMIT 1
    ", [$comp]);
    return $row ? $row['opt'] : '';
  }

  public function pcLogs($comp, $limit = PC_LOG_LIMIT) {
    return DB::all("
      SELECT id, comp, user, opt, val, gtime
      FROM m_logs
      WHERE comp=?
      ORDER BY gtime DESC
      LIMIT ".intval($limit)."
    ", [$comp]);
  }

  public function pcSummary($comp) {
    $lastSeen = DB::val("SELECT MAX(gtime) FROM m_logs WHERE comp=?", [$comp]);
    $opts = DB::all("
      SELECT opt, COUNT(*) cnt, MAX(gtime) last_time
      FROM m_logs
      WHERE comp=?
      GROUP BY opt
      ORDER BY cnt DESC
      LIMIT 30
    ", [$comp]);
    return ['last_seen'=>$lastSeen, 'opts'=>$opts];
  }

  public function quickCounts() {
    // Counts by opt for last 24h (helps dashboard)
    $since = date('Y-m-d H:i:s', strtotime('-24 hours'));
    return DB::all("
      SELECT opt, COUNT(*) cnt
      FROM m_logs
      WHERE gtime >= ?
      GROUP BY opt
      ORDER BY cnt DESC
      LIMIT 30
    ", [$since]);
  }
}

/* -----------------------------
   View (HTML Layout + Components)
------------------------------ */
final class UI {

  public static function layout($title, $bodyHtml) {
    $view = req('view','dashboard');
    $q = h(req('q',''));
    $content = $bodyHtml;

    echo "<!doctype html><html lang='cs'><head>
      <meta charset='utf-8' />
      <meta name='viewport' content='width=device-width, initial-scale=1' />
      <title>".h($title)."</title>
      ".self::css()."
    </head><body>
      <div class='topbar'>
        <div class='brand'>mLogs</div>
        <div class='nav'>
          <a class='".($view==='dashboard'?'active':'')."' href='?view=dashboard'>Dashboard</a>
          <a class='".($view==='logs'?'active':'')."' href='?view=logs'>Log Explorer</a>
          <a class='".($view==='users'?'active':'')."' href='?view=users'>Users</a>
        </div>
        <div class='topsearch'>
          <form method='get' action='' class='topsearchForm'>
            <input type='hidden' name='view' value='logs' />
            <input name='q' value='{$q}' placeholder='Search comp/user/opt/val…' />
            <button class='btn'>Search</button>
          </form>
        </div>
      </div>

      <div class='container'>
        {$content}
      </div>

      ".self::js()."
    </body></html>";
  }

  private static function css() {
    return "<style>
      :root { --bg:#f4f6f8; --card:#fff; --text:#111; --muted:#666; --line:#e8eaed; --accent:#FFEF42; --dark:#222; }
      body{ margin:0; font-family:Arial, sans-serif; background:var(--bg); color:var(--text); }
      .topbar{ position:sticky; top:0; z-index:50; display:flex; align-items:center; gap:16px; padding:10px 14px; background:var(--dark); color:#fff; }
      .brand{ font-weight:800; letter-spacing:.2px; }
      .nav a{ color:#fff; text-decoration:none; padding:6px 10px; border-radius:8px; }
      .nav a.active{ background:#333; outline:1px solid #444; }
      .nav a:hover{ background:#333; }
      .topsearch{ margin-left:auto; }
      .topsearchForm{ display:flex; gap:8px; align-items:center; }
      .topsearchForm input{ width:360px; max-width:45vw; padding:7px 10px; border-radius:10px; border:1px solid #444; background:#111; color:#fff; }
      .btn{ cursor:pointer; border:0; border-radius:10px; padding:8px 12px; background:var(--accent); color:#000; font-weight:700; }
      .btn:hover{ filter:brightness(.95); }
      .container{ width:min(1600px, calc(100% - 24px)); margin:18px auto; }
      .grid{ display:grid; grid-template-columns:repeat(12, 1fr); gap:12px; }
      .card{ background:var(--card); border:1px solid var(--line); border-radius:14px; box-shadow:0 1px 2px rgba(0,0,0,.04); padding:12px; }
      .card h2{ margin:0 0 10px 0; font-size:16px; }
      .kpi{ display:flex; align-items:baseline; gap:8px; }
      .kpi .big{ font-size:22px; font-weight:800; }
      .muted{ color:var(--muted); }
      .row{ display:flex; gap:10px; flex-wrap:wrap; align-items:center; }
      .chip{ display:inline-flex; gap:6px; align-items:center; padding:6px 10px; border-radius:999px; border:1px solid var(--line); background:#fafafa; cursor:pointer; text-decoration:none; color:inherit; }
      .chip:hover{ background:#f5f5f5; }
      .chip b{ font-size:12px; }
      .filters{ display:grid; grid-template-columns:repeat(12,1fr); gap:10px; align-items:end; }
      .field{ grid-column: span 3; }
      .field.sm{ grid-column: span 2; }
      .field.lg{ grid-column: span 6; }
      label{ display:block; font-size:12px; color:var(--muted); margin:0 0 4px 0; }
      input, select{ width:100%; padding:8px 10px; border-radius:10px; border:1px solid var(--line); background:#fff; }
      .actions{ display:flex; gap:8px; align-items:center; }
      .link{ color:#0b57d0; text-decoration:none; }
      .link:hover{ text-decoration:underline; }

      table{ width:100%; border-collapse:collapse; font-size:13px; }
      th,td{ padding:8px 8px; border-bottom:1px solid var(--line); vertical-align:top; }
      th{ position:sticky; top:52px; background:#fff; z-index:10; text-align:left; }
      th .sort{ color:var(--muted); font-size:11px; margin-left:6px; }
      tr:hover td{ background:#fffde6; }
      .mono{ font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, 'Liberation Mono', monospace; white-space: pre-wrap; word-break: break-word; }
      .pre-box{ white-space: pre-wrap; word-break: break-word; max-height: 250px; overflow-y: auto; background: #fafafa; padding: 6px 8px; border: 1px solid var(--line); border-radius: 6px; }
      .nowrap{ white-space:nowrap; }
      .status{ display:inline-block; width:10px; height:10px; border-radius:50%; margin-right:6px; }
      .ok{ background:#2ecc71; }
      .warn{ background:#f1c40f; }
      .bad{ background:#e74c3c; }
      .tag{ font-size:11px; padding:3px 8px; border-radius:999px; border:1px solid var(--line); background:#fafafa; }
      .pager{ display:flex; gap:10px; align-items:center; justify-content:flex-end; margin-top:10px; }
      .pager .btn2{ padding:7px 10px; border-radius:10px; border:1px solid var(--line); background:#fff; cursor:pointer; text-decoration:none; color:inherit; }
      .pager .btn2:hover{ background:#f6f6f6; }

      .col-12{ grid-column: span 12; }
      .col-8{ grid-column: span 8; }
      .col-4{ grid-column: span 4; }
      .col-6{ grid-column: span 6; }
      .col-3{ grid-column: span 3; }

      @media (max-width: 1200px){
        .field, .field.sm, .field.lg { grid-column: span 12; }
        th{ top:52px; }
      }
    </style>";
  }

  private static function js() {
    // AJAX log loading + small helpers
    return "<script>
      function qs(obj){
        const p = new URLSearchParams(obj);
        return p.toString();
      }

      async function apiGet(params){
        const url = location.pathname + '?' + qs(params);
        const r = await fetch(url, { headers: { 'Accept': 'application/json' } });
        return await r.json();
      }

      function setQueryParam(k,v){
        const u = new URL(location.href);
        if(v===null || v==='') u.searchParams.delete(k);
        else u.searchParams.set(k,v);
        history.replaceState({}, '', u);
      }

      async function loadLogsAjax(){
        const box = document.getElementById('logsBox');
        if(!box) return;

        const params = {
          api: 'logs',
          comp: document.getElementById('f_comp')?.value || '',
          user: document.getElementById('f_user')?.value || '',
          opt: document.getElementById('f_opt')?.value || '',
          q: document.getElementById('f_q')?.value || '',
          from: document.getElementById('f_from')?.value || '',
          to: document.getElementById('f_to')?.value || '',
          sort: document.getElementById('f_sort')?.value || 'gtime',
          dir: document.getElementById('f_dir')?.value || 'DESC',
          page: document.getElementById('f_page')?.value || '1',
          limit: document.getElementById('f_limit')?.value || '".DEFAULT_LIMIT."'
        };

        box.innerHTML = '<div class=\"card\"><b>Loading…</b></div>';
        const data = await apiGet(params);
        box.innerHTML = data.html;
      }

      function bindLogs(){
        const form = document.getElementById('logsForm');
        if(!form) return;

        form.addEventListener('submit', (e)=>{ e.preventDefault(); document.getElementById('f_page').value = 1; loadLogsAjax(); });

        const auto = ['f_comp','f_user','f_opt','f_q','f_from','f_to','f_sort','f_dir','f_limit'];
        auto.forEach(id=>{
          const el = document.getElementById(id);
          if(el) el.addEventListener('change', ()=>{ document.getElementById('f_page').value = 1; loadLogsAjax(); });
        });

        const q = document.getElementById('f_q');
        if(q){
          let t=null;
          q.addEventListener('input', ()=>{ clearTimeout(t); t=setTimeout(()=>{ document.getElementById('f_page').value = 1; loadLogsAjax(); }, 250); });
        }

        loadLogsAjax();
      }

      document.addEventListener('DOMContentLoaded', ()=>{
        bindLogs();
      });
    </script>";
  }

  public static function badgeStatusFromLastSeen($lastSeen) {
    if (!$lastSeen) return "<span class='status bad'></span><span class='muted'>unknown</span>";
    $mins = (time() - strtotime($lastSeen)) / 60;
    if ($mins <= 2) return "<span class='status ok'></span><b>online</b>";
    if ($mins <= OFFLINE_MINUTES) return "<span class='status warn'></span><b>late</b>";
    return "<span class='status bad'></span><b>offline</b>";
  }

  public static function statusClassFromLastSeen($lastSeen) {
    if (!$lastSeen) return "bad";
    $mins = (time() - strtotime($lastSeen)) / 60;
    if ($mins <= 2) return "ok";
    if ($mins <= OFFLINE_MINUTES) return "warn";
    return "bad";
  }

  public static function fmtAge($dt) {
    if (!$dt) return "—";
    $sec = time() - strtotime($dt);
    if ($sec < 60) return $sec."s";
    $m = floor($sec/60);
    if ($m < 60) return $m."m";
    $h = floor($m/60);
    if ($h < 48) return $h."h";
    $d = floor($h/24);
    return $d."d";
  }

  public static function updateButton($comp, $version = '') {
    $comp = trim((string)$comp);
    if ($comp === '') return '';
    $outdated = mcwIsOutdatedVersion($version);
    $label = $outdated ? 'Queue update' : 'Run updater';
    $cls = $outdated ? 'btn' : 'btn2';
    return "<form method='post' action='' onsubmit=\"return confirm('Queue updater for ".h($comp)."?');\" style='display:inline-block;margin:0;'>
      <input type='hidden' name='action' value='queueupdate' />
      <input type='hidden' name='comp' value='".h($comp)."' />
      <button class='{$cls}' type='submit'>{$label}</button>
    </form>";
  }

  public static function sysInfoButton($comp) {
    $comp = trim((string)$comp);
    if ($comp === '') return '';
    return "<form method='post' action='' onsubmit=\"return confirm('Request fresh SysInfo (RAM, C:, Printers, Summary) for ".h($comp)."?');\" style='display:inline-block;margin:0;'>
      <input type='hidden' name='action' value='queuesysinfo' />
      <input type='hidden' name='comp' value='".h($comp)."' />
      <button class='btn2' type='submit' title='PostMessage request for #summary, #ram, #free_c, #printers'>Get SysInfo</button>
    </form>";
  }

  public static function customCommandButton($comp) {
    $comp = trim((string)$comp);
    if ($comp === '') return '';
    return "<a class='btn2' href='?view=runcommand&comp=".rawurlencode($comp)."'>Custom command</a>";
  }
}

/* -----------------------------
   Controllers
------------------------------ */
final class Controller {

  private $m;

  public function __construct() { $this->m = new LogModel(); }

  public function dashboard() {
    $userFilter = trim(req('userq',''));
    $compFilter = trim(req('compq',''));
    $pcs = $this->m->latestStatePerPCFiltered(DASHBOARD_LIMIT, $userFilter, $compFilter);
    $counts = $this->m->quickCounts();

    $offline = 0; $online = 0; $late = 0;
    foreach ($pcs as $pc) {
      $mins = $pc['last_seen'] ? (time()-strtotime($pc['last_seen']))/60 : 999999;
      if ($mins <= 2) $online++;
      else if ($mins <= OFFLINE_MINUTES) $late++;
      else $offline++;
    }

    ob_start(); ?>
      <div class="grid">
        <div class="card col-12">
          <h2>Overview</h2>
          <div class="row">
            <span class="chip"><span class="status ok"></span><b><?=h($online)?></b><span class="muted">online (≤ 2 min)</span></span>
            <span class="chip"><span class="status warn"></span><b><?=h($late)?></b><span class="muted">late (≤ <?=OFFLINE_MINUTES?> min)</span></span>
            <span class="chip"><span class="status bad"></span><b><?=h($offline)?></b><span class="muted">offline</span></span>
            <span class="chip"><b><?=h(count($pcs))?></b><span class="muted">PCs shown</span></span>

            <a class="chip" href="?view=logs&opt=ram"><b>RAM logs</b></a>
            <a class="chip" href="?view=logs&opt=free_c"><b>Free C: logs</b></a>
            <a class="chip" href="?view=logs&opt=printers"><b>Printers</b></a>
            <a class="chip" href="?view=logs&opt=summary"><b>Summary logs</b></a>
            <a class="chip" href="?view=logs&opt=ping"><b>Ping logs</b></a>
            <a class="chip" href="?view=logs&opt=ip"><b>IP logs</b></a>
            <a class="chip" href="?view=logs&opt=officeverze"><b>Office</b></a>
            <a class="chip" href="?view=logs&q=error"><b>Search “error”</b></a>
            <a class="chip" href="?view=users"><b>Users view</b></a>
          </div>
        </div>

        <div class="card col-12">
          <h2>Filters</h2>
          <form method="get" class="filters">
            <input type="hidden" name="view" value="dashboard" />
            <div class="field sm">
              <label>Computer contains</label>
              <input name="compq" value="<?=h($compFilter)?>" placeholder="PC name…" />
            </div>
            <div class="field sm">
              <label>Logged user contains</label>
              <input name="userq" value="<?=h($userFilter)?>" placeholder="username…" />
            </div>
            <div class="field sm">
              <label>&nbsp;</label>
              <div class="actions">
                <button class="btn" type="submit">Apply</button>
                <a class="link" href="?view=dashboard">Reset</a>
              </div>
            </div>
          </form>
          <div class="muted" style="margin-top:8px;">Matches against the current heartbeat status, not historical logs.</div>
        </div>

        <div class="card col-4">
          <h2>Stale sessions</h2>
          <div class="row">
            <?php foreach ($pcs as $pc): ?>
              <?php if (!empty($pc['users']) && UI::statusClassFromLastSeen($pc['last_seen']) !== 'ok'): ?>
                <a class="chip" href="<?=h('?view=pc&comp='.$pc['comp'])?>">
                  <span class="status <?=h(UI::statusClassFromLastSeen($pc['last_seen']))?>"></span>
                  <b><?=h($pc['comp'])?></b><span class="muted"><?=h($pc['users'])?></span>
                </a>
              <?php endif; ?>
            <?php endforeach; ?>
          </div>
        </div>

        <div class="card col-4">
          <h2>Last 24h by opt</h2>
          <div class="row">
            <?php foreach ($counts as $c): ?>
              <a class="chip" href="<?=h('?view=logs&opt='.$c['opt'])?>">
                <b><?=h($c['opt'])?></b><span class="muted"><?=h($c['cnt'])?></span>
              </a>
            <?php endforeach; ?>
          </div>
        </div>

        <div class="card col-12">
          <h2>Computers</h2>
          <table>
            <thead>
              <tr>
                <th>Comp</th>
                <th>Status</th>
                <th>Last seen</th>
                <th>Users</th>
                <th>RAM</th>
                <th>Free C:</th>
                <th>Version</th>
                <th>IP</th>
                <th>Ping</th>
                <th>Last log</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              <?php foreach ($pcs as $pc): ?>
                <tr>
                  <td class="nowrap">
                    <a class="link" href="<?=h('?view=pc&comp='.$pc['comp'])?>"><b><?=h($pc['comp'])?></b></a>
                  </td>
                  <td><?=UI::badgeStatusFromLastSeen($pc['last_seen'])?></td>
                  <td class="nowrap"><?=h($pc['last_seen'])?> <span class="muted">(<?=h(UI::fmtAge($pc['last_seen']))?>)</span></td>
                  <td>
                    <?php if (!empty($pc['users'])): ?>
                      <span class="status <?=h(UI::statusClassFromLastSeen($pc['last_seen']))?>"></span><?=h($pc['users'])?>
                    <?php else: ?>
                      <span class="muted">—</span>
                    <?php endif; ?>
                  </td>
                  <td class="mono nowrap" title="<?=h($pc['ram'])?>"><?=h($pc['ram'] ?: '—')?></td>
                  <td class="mono nowrap" title="<?=h($pc['free_c'])?>"><?=h($pc['free_c'] ?: '—')?></td>
                  <td class="mono"><?=h($pc['version'])?></td>
                  <td class="mono"><?=h($pc['ip'])?></td>
                  <td class="mono"><?=h($pc['ping'])?></td>
                  <td>
                    <?php if (!empty($pc['last_opt'])): ?>
                      <span class="tag"><a class="link" href="<?=h('?view=logs&comp='.$pc['comp'].'&opt='.$pc['last_opt'])?>" style="color:inherit;"><?=h($pc['last_opt'])?></a></span>
                      <?php if (!empty($pc['last_val'])): ?>
                        <div class="muted nowrap" style="max-width:220px; overflow:hidden; text-overflow:ellipsis; font-size:11px;" title="<?=h($pc['last_val'])?>">
                          <?=h($pc['last_val'])?>
                        </div>
                      <?php endif; ?>
                    <?php else: ?>
                      <span class="muted">—</span>
                    <?php endif; ?>
                  </td>
                  <td class="nowrap">
                    <?=UI::sysInfoButton($pc['comp'])?>
                    <?=UI::updateButton($pc['comp'], $pc['version'])?>
                    <?=UI::customCommandButton($pc['comp'])?>
                  </td>
                </tr>
              <?php endforeach; ?>
            </tbody>
          </table>
          <div class="muted" style="margin-top:8px;">
            Tip: Click a computer name for detail view.
          </div>
        </div>
      </div>
    <?php
    UI::layout("mLogs - Dashboard", ob_get_clean());
  }

  public function users() {
    $users = $this->m->usersSummary();

    ob_start(); ?>
      <div class="grid">
        <div class="card col-12">
          <h2>Users</h2>
          <div class="muted">Current users based on the latest heartbeat from each computer.</div>
        </div>

        <div class="card col-12">
          <table>
            <thead>
              <tr>
                <th>User</th>
                <th>Computers</th>
                <th>Online</th>
                <th>Late</th>
                <th>Offline</th>
                <th>Last seen</th>
              </tr>
            </thead>
            <tbody>
              <?php foreach ($users as $u): ?>
                <tr>
                  <td class="nowrap"><a class="link" href="<?=h('?view=dashboard&userq='.$u['user'])?>"><?=h($u['user'])?></a></td>
                  <td>
                    <?php foreach ($u['pcs'] as $pc): ?>
                      <a class="chip" href="<?=h('?view=pc&comp='.$pc['comp'])?>">
                        <span class="status <?=h(UI::statusClassFromLastSeen($pc['last_seen']))?>"></span>
                        <b><?=h($pc['comp'])?></b>
                      </a>
                    <?php endforeach; ?>
                  </td>
                  <td><?=h($u['online'])?></td>
                  <td><?=h($u['late'])?></td>
                  <td><?=h($u['offline'])?></td>
                  <td class="nowrap"><?=h($u['last_seen'])?> <span class="muted">(<?=h(UI::fmtAge($u['last_seen']))?>)</span></td>
                </tr>
              <?php endforeach; ?>
            </tbody>
          </table>
        </div>
      </div>
    <?php
    UI::layout("mLogs - Users", ob_get_clean());
  }

  public function runcommand() {
    $comp = req('comp','');
    if (!$comp) {
      UI::layout("mLogs - Run Command", "<div class='card'><b>Missing comp.</b></div>");
      return;
    }

    $program = req('program', MCW_UPDATER_EXE);
    $args = req('args', '');
    $queueErr = req('queueerr','');
    $queued = req('queued','');
    $queuedFile = req('file','');

    ob_start(); ?>
      <div class="grid">
        <?php if ($queued): ?>
          <div class="card col-12">
            <b>Command queued.</b>
            <span class="muted">Command file: <?=h($queuedFile)?></span>
          </div>
        <?php endif; ?>

        <?php if ($queueErr): ?>
          <div class="card col-12">
            <b>Could not queue command.</b>
            <span class="muted"><?=h($queueErr)?></span>
          </div>
        <?php endif; ?>

        <div class="card col-12">
          <h2>Custom Command: <?=h($comp)?></h2>
          <div class="row">
            <a class="chip" href="<?=h('?view=pc&comp='.$comp)?>"><b>Back to PC detail</b></a>
          </div>
        </div>

        <div class="card col-12">
          <form method="post" class="filters">
            <input type="hidden" name="action" value="queuecustomcommand" />
            <input type="hidden" name="comp" value="<?=h($comp)?>" />

            <div class="field lg">
              <label>Program path</label>
              <input name="program" value="<?=h($program)?>" placeholder="\\\\server\\share\\tool.exe" />
            </div>

            <div class="field lg">
              <label>Arguments</label>
              <input name="args" value="<?=h($args)?>" placeholder="Optional arguments" />
            </div>

            <div class="field sm">
              <label>&nbsp;</label>
              <div class="actions">
                <button class="btn" type="submit" onclick="return confirm('Queue this command for <?=h($comp)?>?');">Queue command</button>
                <a class="link" href="<?=h('?view=pc&comp='.$comp)?>">Cancel</a>
              </div>
            </div>
          </form>
          <div class="muted" style="margin-top:8px;">This creates a one-time `.mcw3.xml` task targeted to this computer and runs it as `SYSTEM`.</div>
        </div>
      </div>
    <?php
    UI::layout("mLogs - Run Command - ".$comp, ob_get_clean());
  }

  public function logs() {
    // We render the shell; actual table loads via AJAX ?api=logs
    $opts = $this->m->distinct('opt');
    $comps = $this->m->distinct('comp', 2000);
    $users = $this->m->distinct('user', 2000);

    $opt = req('opt','');
    $comp = req('comp','');
    $user = req('user','');
    $q = req('q','');
    $from = req('from','');
    $to = req('to','');

    $sort = req('sort','gtime');
    $dir = req('dir','DESC');
    $limit = req('limit', DEFAULT_LIMIT);
    $page = req('page', 1);

    ob_start(); ?>
      <div class="grid">
        <div class="card col-12">
          <h2>Log Explorer</h2>

          <form id="logsForm" class="filters">
            <div class="field sm">
              <label>Comp</label>
              <select id="f_comp">
                <option value="">(any)</option>
                <?php foreach($comps as $c): ?>
                  <option value="<?=h($c['v'])?>" <?=($comp===$c['v']?'selected':'')?>><?=h($c['v'])?></option>
                <?php endforeach; ?>
              </select>
            </div>

            <div class="field sm">
              <label>User</label>
              <select id="f_user">
                <option value="">(any)</option>
                <?php foreach($users as $u): ?>
                  <option value="<?=h($u['v'])?>" <?=($user===$u['v']?'selected':'')?>><?=h($u['v'])?></option>
                <?php endforeach; ?>
              </select>
            </div>

            <div class="field sm">
              <label>Opt</label>
              <select id="f_opt">
                <option value="">(any)</option>
                <?php foreach($opts as $o): ?>
                  <option value="<?=h($o['v'])?>" <?=($opt===$o['v']?'selected':'')?>><?=h($o['v'])?></option>
                <?php endforeach; ?>
              </select>
            </div>

            <div class="field lg">
              <label>Search</label>
              <input id="f_q" value="<?=h($q)?>" placeholder="Search in comp/user/opt/val…" />
            </div>

            <div class="field sm">
              <label>From</label>
              <input id="f_from" type="date" value="<?=h($from)?>" />
            </div>

            <div class="field sm">
              <label>To</label>
              <input id="f_to" type="date" value="<?=h($to)?>" />
            </div>

            <div class="field sm">
              <label>Sort</label>
              <select id="f_sort">
                <?php foreach(['gtime','comp','user','opt','val','id'] as $s): ?>
                  <option value="<?=h($s)?>" <?=($sort===$s?'selected':'')?>><?=h($s)?></option>
                <?php endforeach; ?>
              </select>
            </div>

            <div class="field sm">
              <label>Dir</label>
              <select id="f_dir">
                <option value="DESC" <?=($dir==='DESC'?'selected':'')?>>DESC</option>
                <option value="ASC"  <?=($dir==='ASC'?'selected':'')?>>ASC</option>
              </select>
            </div>

            <div class="field sm">
              <label>Limit</label>
              <select id="f_limit">
                <?php foreach([50,100,200,500] as $l): ?>
                  <option value="<?=h($l)?>" <?=((int)$limit===$l?'selected':'')?>><?=h($l)?></option>
                <?php endforeach; ?>
              </select>
            </div>

            <div class="field sm">
              <label>&nbsp;</label>
              <div class="actions">
                <button class="btn" type="submit">Apply</button>
                <a class="link" href="?view=logs">Reset</a>
                <a class="link" href="?view=logs&opt=ram">RAM</a>
                <a class="link" href="?view=logs&opt=free_c">Free C:</a>
                <a class="link" href="?view=logs&opt=printers">Printers</a>
                <a class="link" href="?view=logs&opt=summary">Summary</a>
                <a class="link" href="?view=logs&opt=ping">Ping</a>
                <a class="link" href="?view=logs&opt=ip">IP</a>
                <a class="link" href="?view=logs&opt=officeverze">Office</a>
              </div>
            </div>

            <input type="hidden" id="f_page" value="<?=h($page)?>" />
          </form>
        </div>

        <div class="col-12" id="logsBox"></div>

        <div class="card col-12">
          <h2>Maintenance</h2>
          <div class="row">
            <a class="chip" href="?action=removeold&view=logs" onclick="return confirm('Remove old & duplicates?');"><b>Remove old (7 days) + dedupe</b></a>
            <span class="muted">Uses your existing cleanup logic.</span>
          </div>
        </div>
      </div>
    <?php
    UI::layout("mLogs - Log Explorer", ob_get_clean());
  }

  public function pc() {
    $comp = req('comp','');
    if (!$comp) {
      UI::layout("mLogs - PC", "<div class='card'><b>Missing comp.</b></div>");
      return;
    }

    $summary = $this->m->pcSummary($comp);
    $logs = $this->m->pcLogs($comp);
    $status = $this->m->pcStatus($comp);

    // Quick latest
    $ip = $status ? $status['ip_addresses'] : $this->m->latestVal($comp,'ip');
    $ping = $status ? $status['ping_ms'] : $this->m->latestVal($comp,'ping');
    $office = $this->m->latestVal($comp,'officeverze');
    $sig = $this->m->latestVal($comp,'sigmakra');
    $ram = $this->m->latestVal($comp, 'ram');
    $freeC = $this->m->latestVal($comp, 'free_c');
    if (!$freeC) $freeC = $this->m->latestVal($comp, 'disk_c');
    if (!$freeC) $freeC = $this->m->latestVal($comp, 'disk');
    $printers = $this->m->latestVal($comp, 'printers');
    $summaryInfo = $this->m->latestVal($comp, 'summary');
    if (!$summaryInfo) $summaryInfo = $this->m->latestVal($comp, 'info');
    $users = $status ? $status['logged_users'] : '';
    $version = $status ? $status['client_version'] : '';
    $context = $status ? $status['agent_context'] : '';
    $osVersion = $status ? $status['os_version'] : '';
    $bootTime = $status ? $status['boot_time'] : '';
    $lastSeen = $status && !empty($status['last_seen']) ? $status['last_seen'] : $summary['last_seen'];
    $isOutdated = mcwIsOutdatedVersion($version);
    $queued = req('queued','');
    $queuedFile = req('file','');
    $queueErr = req('queueerr','');

    ob_start(); ?>
      <div class="grid">
        <?php if ($queued): ?>
          <div class="card col-12">
            <b>Update queued.</b>
            <span class="muted">Command file: <?=h($queuedFile)?></span>
          </div>
        <?php endif; ?>

        <?php if ($queueErr): ?>
          <div class="card col-12">
            <b>Could not queue update.</b>
            <span class="muted"><?=h($queueErr)?></span>
          </div>
        <?php endif; ?>

        <div class="card col-12">
          <h2>Computer: <?=h($comp)?></h2>
          <div class="row">
            <span class="chip"><?=UI::badgeStatusFromLastSeen($lastSeen)?></span>
            <span class="chip"><b>Last seen</b> <span class="muted"><?=h($lastSeen)?> (<?=h(UI::fmtAge($lastSeen))?>)</span></span>
            <?php if ($version !== ''): ?>
              <span class="chip">
                <b>Version</b>
                <span class="muted"><?=h($version)?><?= $isOutdated ? ' (old)' : '' ?></span>
              </span>
            <?php endif; ?>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp)?>"><b>Open in Log Explorer</b></a>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp.'&opt=ram')?>"><b>RAM</b></a>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp.'&opt=free_c')?>"><b>Free C:</b></a>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp.'&opt=printers')?>"><b>Printers</b></a>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp.'&opt=summary')?>"><b>Summary</b></a>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp.'&opt=ping')?>"><b>Ping only</b></a>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp.'&opt=ip')?>"><b>IP only</b></a>
            <a class="chip" href="<?=h('?view=logs&comp='.$comp.'&opt=officeverze')?>"><b>Office only</b></a>
            <?=UI::sysInfoButton($comp)?>
            <?=UI::updateButton($comp, $version)?>
            <?=UI::customCommandButton($comp)?>
          </div>
        </div>

        <div class="card col-6">
          <h2>Latest state</h2>
          <table>
            <tr><td class="muted">Users</td><td><?=h($users)?></td></tr>
            <tr><td class="muted">Client version</td><td class="mono"><?=h($version)?></td></tr>
            <tr><td class="muted">Agent context</td><td><?=h($context)?></td></tr>
            <tr><td class="muted">IP</td><td class="mono"><?=h($ip)?></td></tr>
            <tr><td class="muted">Ping</td><td class="mono"><?=h($ping)?></td></tr>
            <tr><td class="muted">OS</td><td><?=h($osVersion)?></td></tr>
            <tr><td class="muted">Boot time</td><td class="nowrap"><?=h($bootTime)?></td></tr>
            <?php if (!empty($ram)): ?>
              <tr><td class="muted">RAM</td><td class="mono"><?=h($ram)?></td></tr>
            <?php endif; ?>
            <?php if (!empty($freeC)): ?>
              <tr><td class="muted">Free C:</td><td class="mono"><?=h($freeC)?></td></tr>
            <?php endif; ?>
            <?php if (!empty($printers)): ?>
              <tr><td class="muted">Printers</td><td><div class="mono pre-box"><?=h($printers)?></div></td></tr>
            <?php endif; ?>
            <?php if (!empty($summaryInfo)): ?>
              <tr><td class="muted">Summary / Info</td><td><div class="mono pre-box"><?=h($summaryInfo)?></div></td></tr>
            <?php endif; ?>
            <tr><td class="muted">Office</td><td><?=h($office)?></td></tr>
            <tr><td class="muted">Sig</td><td class="mono"><?=h($sig)?></td></tr>
          </table>
        </div>

        <div class="card col-6">
          <h2>Opt summary</h2>
          <table>
            <thead><tr><th>Opt</th><th>Count</th><th>Last</th></tr></thead>
            <tbody>
              <?php foreach($summary['opts'] as $o): ?>
                <tr>
                  <td><a class="link" href="<?=h('?view=logs&comp='.$comp.'&opt='.$o['opt'])?>"><?=h($o['opt'])?></a></td>
                  <td><?=h($o['cnt'])?></td>
                  <td class="nowrap"><?=h($o['last_time'])?></td>
                </tr>
              <?php endforeach; ?>
            </tbody>
          </table>
        </div>

        <div class="card col-12">
          <h2>Recent logs (<?=h(count($logs))?>)</h2>
          <table>
            <thead>
              <tr>
                <th>Time</th>
                <th>Opt</th>
                <th>User</th>
                <th>Val</th>
              </tr>
            </thead>
            <tbody>
              <?php foreach($logs as $r): ?>
                <tr>
                  <td class="nowrap"><?=h($r['gtime'])?></td>
                  <td><span class="tag"><?=h($r['opt'])?></span></td>
                  <td><?=h($r['user'])?></td>
                  <td class="mono"><?=h($r['val'])?></td>
                </tr>
              <?php endforeach; ?>
            </tbody>
          </table>
        </div>
      </div>
    <?php
    UI::layout("mLogs - ".$comp, ob_get_clean());
  }

  /* -----------------------------
     API endpoints (JSON)
  ------------------------------ */
  public function apiLogs() {
    // Filters
    $filters = [
      'comp' => req('comp',''),
      'user' => req('user',''),
      'opt'  => req('opt',''),
      'q'    => req('q',''),
      'from' => req('from',''),
      'to'   => req('to',''),
    ];

    $from = req('from','');
    $to = req('to','');
    $filters['from'] = $from ? toDateTimeOrEmpty($from) : '';
    $filters['to']   = $to ? date('Y-m-d 23:59:59', strtotime($to)) : '';

    $sort = req('sort','gtime');
    $dir = req('dir','DESC');
    $limit = max(1, min(500, intval(req('limit', DEFAULT_LIMIT))));
    $page = max(1, intval(req('page', 1)));
    $offset = ($page - 1) * $limit;

    [$rows, $total] = $this->m->searchLogs($filters, $sort, $dir, $limit, $offset);

    $pages = max(1, (int)ceil($total / $limit));

    // Render HTML fragment for table + pager
    ob_start(); ?>
      <div class="card">
        <div class="row" style="justify-content:space-between;">
          <div>
            <b><?=h($total)?></b> results
            <span class="muted">• page <?=h($page)?> / <?=h($pages)?></span>
          </div>
          <div class="row">
            <span class="muted">Tip: click comp to open PC detail.</span>
          </div>
        </div>

        <table>
          <thead>
            <tr>
              <th>ID</th>
              <th>Comp</th>
              <th>User</th>
              <th>Opt</th>
              <th>Val</th>
              <th>Time</th>
            </tr>
          </thead>
          <tbody>
            <?php foreach($rows as $r): ?>
              <tr>
                <td class="mono"><?=h($r['id'])?></td>
                <td class="nowrap"><a class="link" href="<?=h('?view=pc&comp='.$r['comp'])?>"><?=h($r['comp'])?></a></td>
                <td><?=h($r['user'])?></td>
                <td><span class="tag"><?=h($r['opt'])?></span></td>
                <td class="mono"><?=h($r['val'])?></td>
                <td class="nowrap"><?=h($r['gtime'])?></td>
              </tr>
            <?php endforeach; ?>
          </tbody>
        </table>

        <div class="pager">
          <?php
            $prev = max(1, $page-1);
            $next = min($pages, $page+1);
          ?>
          <button class="btn2" onclick="document.getElementById('f_page').value=1; loadLogsAjax();">⟪ First</button>
          <button class="btn2" onclick="document.getElementById('f_page').value='<?=h($prev)?>'; loadLogsAjax();">‹ Prev</button>
          <span class="muted">Page</span>
          <input style="width:80px" value="<?=h($page)?>" onkeydown="if(event.key==='Enter'){ document.getElementById('f_page').value=this.value; loadLogsAjax(); }" />
          <span class="muted">of <?=h($pages)?></span>
          <button class="btn2" onclick="document.getElementById('f_page').value='<?=h($next)?>'; loadLogsAjax();">Next ›</button>
          <button class="btn2" onclick="document.getElementById('f_page').value='<?=h($pages)?>'; loadLogsAjax();">Last ⟫</button>
        </div>
      </div>
    <?php

    $html = ob_get_clean();
    header('Content-Type: application/json; charset=utf-8');
    echo json_encode(['ok'=>true, 'html'=>$html, 'total'=>$total, 'page'=>$page, 'pages'=>$pages]);
  }
}

/* -----------------------------
   Legacy & Service actions
------------------------------ */
$action = req('action','');
$doLoad = req('doLoad','');
$purge7 = req('purge7','');
$controller = new Controller();
$model = new LogModel();

if ($purge7) {
  $model->removeOld(7);
  echo "purge ok";
  exit;
}

if ($doLoad) {
  $model->loadFiles('data');
  $model->removeOld(7);
  header("Location: ?view=dashboard");
  exit;
}

if ($action === 'putlog') {
  // Keep endpoint compatible; support widget and message aliases from PostMessage
  $comp = req('comp','');
  $user = req('user','');
  $opt  = req('opt', req('widget',''));
  $val  = req('val', req('message',''));
  $dt   = req('dt','');

  $id = $model->putLog($comp, $user, $opt, $val, $dt);

  // legacy debug file
  if (!is_dir("data/nfo")) @mkdir("data/nfo", 0777, true);
  $fp = @fopen("data/nfo/putlog.txt","a+");
  if ($fp) { fwrite($fp, "put log at: ".date("Y-m-d H:i:s")." id=$id comp=$comp user=$user opt=$opt\n"); fclose($fp); }

  echo "log put ok $id";
  exit;
}

if ($action === 'queuesysinfo') {
  $comp = req('comp','');
  try {
    $fileName = $model->queueSysInfoCommand($comp);
    header("Location: ?view=pc&comp=".urlencode($comp)."&queued=1&file=".urlencode($fileName));
  } catch (Exception $ex) {
    header("Location: ?view=pc&comp=".urlencode($comp)."&queueerr=".urlencode($ex->getMessage()));
  }
  exit;
}

if ($action === 'heartbeat') {
  $id = $model->putHeartbeat([
    'comp' => req('comp',''),
    'users' => req('users',''),
    'client_version' => req('client_version',''),
    'ip_addresses' => req('ip_addresses',''),
    'ping_ms' => req('ping_ms',''),
    'agent_context' => req('agent_context',''),
    'os_version' => req('os_version',''),
    'boot_time' => req('boot_time',''),
    'dt' => req('dt',''),
  ]);

  if (!is_dir("data/nfo")) @mkdir("data/nfo", 0777, true);
  $fp = @fopen("data/nfo/heartbeat.txt","a+");
  if ($fp) {
    fwrite($fp, "heartbeat at: ".date("Y-m-d H:i:s")." id=$id comp=".req('comp','')." users=".req('users','')."\n");
    fclose($fp);
  }

  echo "heartbeat ok";
  exit;
}

if ($action === 'queueupdate') {
  $comp = req('comp','');
  try {
    $fileName = $model->queueRunProgramCommand($comp, MCW_UPDATER_EXE);
    header("Location: ?view=pc&comp=".urlencode($comp)."&queued=1&file=".urlencode($fileName));
  } catch (Exception $ex) {
    header("Location: ?view=pc&comp=".urlencode($comp)."&queueerr=".urlencode($ex->getMessage()));
  }
  exit;
}

if ($action === 'queuecustomcommand') {
  $comp = req('comp','');
  $program = req('program','');
  $args = req('args','');
  try {
    $fileName = $model->queueCustomRunCommand($comp, $program, $args);
    header("Location: ?view=runcommand&comp=".urlencode($comp)."&queued=1&file=".urlencode($fileName)."&program=".urlencode($program)."&args=".urlencode($args));
  } catch (Exception $ex) {
    header("Location: ?view=runcommand&comp=".urlencode($comp)."&queueerr=".urlencode($ex->getMessage())."&program=".urlencode($program)."&args=".urlencode($args));
  }
  exit;
}

if ($action === 'removeold') {
  $model->removeOld(7);
  // after cleanup go back
  header("Location: ?view=logs");
  exit;
}

/* -----------------------------
   API Router
------------------------------ */
if (req('api','') === 'logs') {
  $controller->apiLogs();
  exit;
}

/* -----------------------------
   Page Router
------------------------------ */
$view = req('view','dashboard');
if ($view === 'logs') { $controller->logs(); exit; }
if ($view === 'pc')   { $controller->pc(); exit; }
if ($view === 'users') { $controller->users(); exit; }
if ($view === 'runcommand') { $controller->runcommand(); exit; }
$controller->dashboard();
