using GuideViewer.Data.Entities;
using GuideViewer.Data.Repositories;
using Serilog;
using System;
using System.Collections.Generic;

namespace GuideViewer.Core.Utilities;

/// <summary>
/// Utility for seeding sample data into the database for testing.
/// </summary>
public static class SampleDataSeeder
{
    /// <summary>
    /// Seeds sample categories and guides into the database.
    /// </summary>
    public static void SeedSampleData(CategoryRepository categoryRepository, GuideRepository guideRepository)
    {
        try
        {
            // Check if data already exists
            var existingGuides = guideRepository.GetAll();
            if (System.Linq.Enumerable.Any(existingGuides))
            {
                Log.Information("Sample data already exists, skipping seeding");
                return;
            }

            Log.Information("Seeding sample data...");

            // Create categories
            var categories = new List<Category>
            {
                new Category
                {
                    Name = "Network Installation",
                    Description = "Guides for installing network equipment",
                    IconGlyph = "\uE968",
                    Color = "#0078D4"
                },
                new Category
                {
                    Name = "Server Setup",
                    Description = "Server installation and configuration guides",
                    IconGlyph = "\uE9F3",
                    Color = "#107C10"
                },
                new Category
                {
                    Name = "Software Deployment",
                    Description = "Application and software installation guides",
                    IconGlyph = "\uECAA",
                    Color = "#8764B8"
                },
                new Category
                {
                    Name = "Hardware Maintenance",
                    Description = "Hardware repair and maintenance procedures",
                    IconGlyph = "\uE90F",
                    Color = "#D13438"
                }
            };

            foreach (var category in categories)
            {
                categoryRepository.Insert(category);
            }

            // Create sample guides
            var guides = new List<Guide>
            {
                new Guide
                {
                    Title = "Installing a Cisco Router",
                    Description = "Complete guide for installing and configuring a Cisco enterprise router including network setup and security configuration.",
                    Category = "Network Installation",
                    EstimatedMinutes = 45,
                    CreatedBy = "Admin",
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Order = 1,
                            Title = "Unbox and inspect equipment",
                            Content = "Carefully unbox the router and verify all components are present: router unit, power cable, console cable, mounting brackets, and documentation."
                        },
                        new Step
                        {
                            Order = 2,
                            Title = "Mount the router",
                            Content = "Install the router in the network rack using the provided mounting brackets. Ensure proper ventilation around the unit."
                        },
                        new Step
                        {
                            Order = 3,
                            Title = "Connect power and console cable",
                            Content = "Connect the power cable and console cable. Do not power on the unit yet."
                        },
                        new Step
                        {
                            Order = 4,
                            Title = "Connect network cables",
                            Content = "Connect WAN and LAN network cables according to the network diagram."
                        },
                        new Step
                        {
                            Order = 5,
                            Title = "Power on and configure",
                            Content = "Power on the router and access the console to perform initial configuration using the setup wizard."
                        }
                    }
                },
                new Guide
                {
                    Title = "Windows Server 2022 Installation",
                    Description = "Step-by-step guide for installing Windows Server 2022 on physical or virtual hardware with best practices.",
                    Category = "Server Setup",
                    EstimatedMinutes = 60,
                    CreatedBy = "Admin",
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Order = 1,
                            Title = "Verify system requirements",
                            Content = "Ensure the server meets minimum requirements: 1.4 GHz 64-bit processor, 512 MB RAM (2 GB recommended), 32 GB disk space."
                        },
                        new Step
                        {
                            Order = 2,
                            Title = "Create installation media",
                            Content = "Download Windows Server 2022 ISO and create bootable USB drive using Rufus or similar tool."
                        },
                        new Step
                        {
                            Order = 3,
                            Title = "Boot from installation media",
                            Content = "Insert USB drive and boot server from USB. Press appropriate key to enter boot menu if needed."
                        },
                        new Step
                        {
                            Order = 4,
                            Title = "Run Windows Setup",
                            Content = "Select language, time format, and keyboard. Click 'Install Now' and enter product key when prompted."
                        },
                        new Step
                        {
                            Order = 5,
                            Title = "Configure installation options",
                            Content = "Choose installation type (Desktop Experience or Server Core), select installation drive, and wait for installation to complete."
                        },
                        new Step
                        {
                            Order = 6,
                            Title = "Initial configuration",
                            Content = "Set administrator password, configure computer name, and join domain if applicable."
                        }
                    }
                },
                new Guide
                {
                    Title = "Microsoft Office 365 Deployment",
                    Description = "Enterprise deployment guide for Microsoft Office 365 ProPlus using configuration XML and deployment tools.",
                    Category = "Software Deployment",
                    EstimatedMinutes = 30,
                    CreatedBy = "Admin",
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Order = 1,
                            Title = "Download Office Deployment Tool",
                            Content = "Download the latest Office Deployment Tool (ODT) from Microsoft Download Center."
                        },
                        new Step
                        {
                            Order = 2,
                            Title = "Create configuration XML",
                            Content = "Create a configuration.xml file specifying Office apps, update channel, and language settings."
                        },
                        new Step
                        {
                            Order = 3,
                            Title = "Download Office files",
                            Content = "Run 'setup.exe /download configuration.xml' to download Office installation files to local cache."
                        },
                        new Step
                        {
                            Order = 4,
                            Title = "Deploy Office",
                            Content = "Run 'setup.exe /configure configuration.xml' to install Office 365 on target machines."
                        }
                    }
                },
                new Guide
                {
                    Title = "Workstation RAM Upgrade",
                    Description = "Procedure for upgrading RAM in desktop workstations including compatibility check and installation.",
                    Category = "Hardware Maintenance",
                    EstimatedMinutes = 20,
                    CreatedBy = "Admin",
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Order = 1,
                            Title = "Verify RAM compatibility",
                            Content = "Check motherboard specifications to determine supported RAM type (DDR4/DDR5), speed, and maximum capacity."
                        },
                        new Step
                        {
                            Order = 2,
                            Title = "Power down and unplug",
                            Content = "Shut down the workstation completely and disconnect power cable. Press power button to discharge residual power."
                        },
                        new Step
                        {
                            Order = 3,
                            Title = "Install RAM modules",
                            Content = "Open case, locate RAM slots, align RAM module notches with slot, and press firmly until clips snap into place."
                        },
                        new Step
                        {
                            Order = 4,
                            Title = "Verify installation",
                            Content = "Power on workstation and verify RAM is detected in BIOS. Run memory diagnostic tool to test stability."
                        }
                    }
                },
                new Guide
                {
                    Title = "Network Switch Configuration",
                    Description = "Initial configuration guide for managed network switches including VLAN setup and security hardening.",
                    Category = "Network Installation",
                    EstimatedMinutes = 40,
                    CreatedBy = "Admin",
                    Steps = new List<Step>
                    {
                        new Step
                        {
                            Order = 1,
                            Title = "Physical installation",
                            Content = "Mount switch in rack and connect power. Connect console cable to management port."
                        },
                        new Step
                        {
                            Order = 2,
                            Title = "Access console",
                            Content = "Connect to switch via console using terminal emulator (PuTTY) with correct baud rate (usually 9600)."
                        },
                        new Step
                        {
                            Order = 3,
                            Title = "Basic configuration",
                            Content = "Set hostname, configure management IP address, and set enable password."
                        },
                        new Step
                        {
                            Order = 4,
                            Title = "Create VLANs",
                            Content = "Create necessary VLANs and assign ports to appropriate VLANs based on network design."
                        },
                        new Step
                        {
                            Order = 5,
                            Title = "Enable security features",
                            Content = "Configure port security, disable unused ports, and enable SSH for remote management."
                        }
                    }
                }
            };

            foreach (var guide in guides)
            {
                guideRepository.Insert(guide);
            }

            Log.Information("Sample data seeded successfully: {CategoryCount} categories, {GuideCount} guides",
                categories.Count, guides.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to seed sample data");
        }
    }
}
