# Étude de Cas Technique - Intégration SharePoint Online & Azure

Bienvenue sur le dépôt de mon étude de cas technique. Ce projet démontre une intégration complète entre SharePoint Online et Azure.

L'objectif est simple : une application Console initialise une liste SharePoint avec des données, et un Web Part SPFx permet aux utilisateurs d'envoyer ces données vers une Azure Function pour traitement.

## Architecture du Projet

La solution est divisée en trois briques indépendantes :

1. **CaseStudyConsole** : Un outil en C# (.NET 8) qui utilise le SDK **PnP Core**. Il se connecte à votre tenant, crée automatiquement la liste `CaseStudyList` et y injecte les données de test.
2. **CaseStudyBackend** : Une Azure Function (HTTP Trigger) construite en .NET 8 (modèle Isolated Worker). Elle joue le rôle d'API pour recevoir les données.
3. **CaseStudySPFx** : Le composant frontal (Web Part React). Il s'intègre à l'interface SharePoint, lit la liste et communique avec le backend.

## Prérequis

Avant de lancer le projet, assurez-vous d'avoir l'environnement suivant :

* **.NET 8.0 SDK** (pour la Console et le Backend).
* **Node.js v18** (Recommandé pour SPFx. *Attention : Node v22 n'est pas encore supporté*).
* **Azure Functions Core Tools v4**.
* Un accès administrateur à un tenant SharePoint Online.

---

## Guide de Démarrage

Suivez ces étapes dans l'ordre pour garantir le bon fonctionnement.

### Étape 1 : Configuration Azure (Indispensable)
Pour que l'application Console puisse toucher à votre SharePoint, elle a besoin d'une identité :
1. Allez sur le portail Azure > **App Registrations**.
2. Créez une nouvelle application.
3. Dans **API Permissions**, ajoutez `SharePoint` > `Delegated` > `AllSites.FullControl` (ou `AllSites.Write`).
4. **Important :** N'oubliez pas de cliquer sur le bouton **"Grant Admin Consent"**.
5. Notez votre `Application (client) ID` et votre `Directory (tenant) ID`.

### Étape 2 : Lancer le Backend
C'est le serveur qui va écouter les requêtes.
1. Ouvrez un terminal dans le dossier `CaseStudyBackend`.
2. Lancez la commande : `func start`
3. Gardez ce terminal ouvert. L'API écoute désormais sur `http://localhost:7071/api/ReceiveItem`.

### Étape 3 : Initialiser les Données (Console App)
J'ai rendu cette partie dynamique pour qu'elle fonctionne sur n'importe quel tenant.
1. Ouvrez un terminal dans `CaseStudyConsole`.
2. Lancez : `dotnet run`
3. L'application vous demandera d'entrer les IDs récupérés à l'étape 1 ainsi que l'URL de votre site SharePoint.
4. Une fois terminé, vérifiez sur votre site : la liste `CaseStudyList` devrait contenir 5 éléments.

### Étape 4 : Lancer le Web Part (SPFx)
1. Allez dans le dossier `CaseStudySPFx`.
2. Installez les dépendances : `npm install`
3. **Configuration :** Ouvrez le fichier `config/serve.json` et remplacez l'URL `initialPage` par celle de votre tenant (`https://votre-tenant.sharepoint.com/_layouts/15/workbench.aspx`).
4. Démarrez le serveur : `gulp serve`
5. Le Workbench SharePoint s'ouvrira automatiquement. Ajoutez le Web Part **CaseStudySender** et testez l'envoi !

---

## Choix Techniques

* **PnP Core SDK** : Choisi pour sa simplicité et sa robustesse face aux appels API SharePoint natifs.
* **Isolated Worker** : J'ai opté pour ce modèle pour l'Azure Function afin d'avoir un contrôle total sur l'injection de dépendances et pour être aligné avec les standards .NET 8.
* **Sécurité** : Le code ne contient aucun secret en dur (hardcoded). Tout est demandé à l'exécution ou configuré localement.
